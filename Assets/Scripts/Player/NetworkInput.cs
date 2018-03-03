#region

using System.Collections.Generic;
using DemoGame.Camera;
using DemoGame.Player.Input;
using UnityEngine;
using UnityEngine.Networking;

#endregion

namespace DemoGame.Player
{
    /// <summary>
    ///     This script manage the following parts of the communication
    ///     - Client create input state at fixedDeltaTime rate
    ///     - Client simulate player at fixedDeltaTime rate
    ///     - Client send non ACK states every 0.33 seconds
    ///     - Server receive states and simulate them
    ///     - Server ACK states and send simulation result
    ///     - Client receive result (true position)
    ///     - Client play back all unACKed states on top of the new true state
    ///     - Client interpolate smoothly between predicted server state and his own state
    /// </summary>
    /// <remarks>
    ///     Updates are send on channel 1 (Unreliable)
    /// </remarks>
    [NetworkSettings(channel = 1, sendInterval = 0.33f)]
    public class NetworkInput : NetworkBehaviour
    {
        //How many states to keep before warning
        private const float WarningClientWaitingStates = 30;

        //How many states to keep on client
        private const float MaxClientWaitingStates = 50;

        //Max distance between server and localy calculated position
        private const float MaxClientDistanceWarning = 0.25f;

        //Max distance between client and server calculated position before SNAPPING
        private const float MaxServerDistanceSnap = 0.15f;

        private AimPoint _cameraAimPoint;

        //Others characters scripts (see each script to know what it does)
        private MouseAim _cameraMouseAim;
        private InputManager _characterInput;
        private Movement _characterMovement;
        private Rotation _characterRotation;

        //CLIENT SIDE last ack state
        [SerializeField] private int _clientAckState;

        //SERVER SIDE last received state
        [SerializeField] private int _clientInputState;

        //CLIENT SIDE input states not ack by server
        private Queue<Input.State> _inputStates;

        //CLIENT SIDE last sended state
        [SerializeField] private int _localInputState;

        //CLIENT SIDE time when the client must send data to server
        private float _nextSendTime;

        //CLIENT SIDE last predicted pos from server
        private Vector3 _serverLastPredPosition;

        //CLIENT SIDE last predicted rot from server
        private Quaternion _serverLastPredRotation;

        //CLIENT SIDE last received pos from server
        private Vector3 _serverLastRecvPosition;

        //CLIENT SIDE last received rot from server
        private Quaternion _serverLastRecvRotation;

        private void Start()
        {
            _inputStates = new Queue<Input.State>();

            _cameraMouseAim = UnityEngine.Camera.main.GetComponent<MouseAim>();
            _cameraAimPoint = UnityEngine.Camera.main.GetComponent<AimPoint>();

            _characterInput = GetComponent<InputManager>();
            _characterMovement = GetComponent<Movement>();
            _characterRotation = GetComponent<Rotation>();
        }

        private void FixedUpdate()
        {
            //Client: Please read: http://forum.unity3d.com/threads/tips-for-server-authoritative-player-movement.199538/

            //Client: Only client run simulation in realtime for the player to see

            if (!isLocalPlayer)
                return;

            //Client: start a new state
            _localInputState = _localInputState + 1;

            //Client: Updates camera
            _cameraMouseAim.RunUpdate(Time.fixedDeltaTime);
            _cameraAimPoint.RunUpdate(Time.fixedDeltaTime);

            //Client: gathers user input state
            _characterInput.Parse(_localInputState);

            //Client: add new input to the list
            _inputStates.Enqueue(_characterInput.CurrentInput);

            //Client: execute simulation on local data
            _characterMovement.RunUpdate(Time.fixedDeltaTime);
            _characterRotation.RunUpdate(Time.fixedDeltaTime);

            //Client: Trim commands to 25 and send commands to server
            if (_inputStates.Count > WarningClientWaitingStates)
                Debug.LogWarning("[NetworkInput]: States starting pulling up, are network condition bad?");

            if (_inputStates.Count > MaxClientWaitingStates)
                Debug.LogError("Too many waiting states, starting to drop frames");

            while (_inputStates.Count > MaxClientWaitingStates)
                _inputStates.Dequeue();

            //Client: Send every sendInterval
            if (isServer && isLocalPlayer || _nextSendTime < Time.time)
            {
                CmdSetServerInput(_inputStates.ToArray(), transform.position);
                _nextSendTime = Time.time + 0.33f;
            }
        }

        /// <summary>
        ///     Server: Receive a list of inputs from the client
        /// </summary>
        /// <param name="newInputs"></param>
        [Command(channel = 1)]
        private void CmdSetServerInput(Input.State[] newInputs, Vector3 newClientPos)
        {
            var index = 0;

            //Server: Input received but state not consecutive with the last one ACKed
            if (newInputs.Length > 0 && newInputs[index].InputState > _clientInputState + 1)
                Debug.LogWarning("Missing inputs from " + _clientInputState + " to " + newInputs[index].InputState);

            //Server: Discard all old states (state already ACK from the server)
            while (index < newInputs.Length && newInputs[index].InputState <= _clientInputState)
                index++;

            //Server: Run through all received states to execute them
            while (index < newInputs.Length)
            {
                //Server: Set the character input
                _characterInput.CurrentInput = newInputs[index];

                //Server: Set the client state number
                _clientInputState = newInputs[index].InputState;

                //Server: Run update for this step according to received input
                if (!isLocalPlayer)
                {
                    _characterMovement.RunUpdate(Time.fixedDeltaTime);
                    _characterRotation.RunUpdate(Time.fixedDeltaTime);
                }

                index++;
            }

            //Check on server that position received from client isn't too far from the position calculated locally
            //TODO: maybe add a cheat check here
            if (Vector3.Distance(newClientPos, transform.position) > MaxClientDistanceWarning)
                Debug.LogWarning(
                    "Client distance too far from player (maybe net condition are very bad or move code isn't deterministic)");

            //Server: Send to other script that state update finished
            SendMessage("ServerStateReceived", _clientInputState, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        ///     Receive a good state from the server
        ///     Discard input older than this good state
        ///     Replay missing inputs on top of it
        /// </summary>
        /// <param name="serverRecvState"></param>
        /// <param name="serverRecvPosition"></param>
        /// <param name="serverRecvRotation"></param>
        private void ServerState(State characterState)
        {
            var serverRecvState = characterState.Frame;
            var serverRecvPosition = characterState.Position;
            var serverRecvRotation = characterState.Rotation;

            //Client: Check that we received a new state from server (not some delayed packet)
            if (_clientAckState >= serverRecvState)
                return;
            //Client: Set the last server ack state
            _clientAckState = serverRecvState;

            //Client: Discard all input states where state are before the ack state
            var loop = true;
            while (loop && _inputStates.Count > 0)
            {
                var state = _inputStates.Peek();
                if (state.InputState <= _clientAckState)
                    _inputStates.Dequeue();
                else
                    loop = false;
            }

            //Client: store actual Player position, rotation and velocity along with current input
            var oldState = _characterInput.CurrentInput;
            var oldPos = transform.position;
            var oldRot = transform.rotation;

            //Client: move back the player to the received server position
            _serverLastRecvPosition = serverRecvPosition;
            _serverLastRecvRotation = serverRecvRotation;
            transform.position = _serverLastRecvPosition;
            transform.rotation = _serverLastRecvRotation;

            _characterMovement.IsReplayMovement = true;

            //Client: replay all input based on new correct position
            foreach (var state in _inputStates)
            {
                //Set the input
                _characterInput.CurrentInput = state;
                
                //Run the simulation
                _characterMovement.RunUpdate(Time.fixedDeltaTime);
                _characterRotation.RunUpdate(Time.fixedDeltaTime);
            }

            _characterMovement.IsReplayMovement = false;

            //Client: save the new predicted character position
            _serverLastPredPosition = transform.position;
            _serverLastPredRotation = transform.rotation;

            //Client: restore initial position, rotation and velocity
            _characterInput.CurrentInput = oldState;
            transform.position = oldPos;
            transform.rotation = oldRot;

            //Client: Check if a prediction error occured in the past
            //Debug.Log("States in queue: " + inputStates.Count + " Predicted distance: " + Vector3.Distance(transform.position, serverLastPredPosition));
            if (Vector3.Distance(transform.position, _serverLastPredPosition) > MaxServerDistanceSnap)
            {
                //Client: Snap to correct position
                Debug.LogWarning("Prediction error!");

                transform.position = Vector3.Lerp(transform.position, _serverLastPredPosition,
                    Time.fixedDeltaTime * 10);

                transform.rotation = Quaternion.Lerp(transform.rotation, _serverLastPredRotation,
                    Time.fixedDeltaTime * 10);
            }
        }

        private void OnDrawGizmos()
        {
            if (isServer)
            {
            }
            else if (isLocalPlayer)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_serverLastRecvPosition + Vector3.up, Vector3.one + Vector3.up);

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_serverLastPredPosition + Vector3.up, Vector3.one + Vector3.up);
            }
        }
    }
}