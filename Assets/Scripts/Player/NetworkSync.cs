using UnityEngine;
using UnityEngine.Networking;

namespace DemoGame.Player
{
    /// <summary>
    ///     For local player, dispatch received position to CharacterNetworkInput
    ///     For distant player, do interpolation between received states
    /// </summary>
    [NetworkSettings(channel = 1, sendInterval = 0.33f)]
    public class NetworkSync : NetworkBehaviour
    {
        private NetworkInterpolation networkInterpolation; //The interpolation component

        [SyncVar] private State serverLastState; //SERVER: Store last state

        private void Start()
        {
            networkInterpolation = GetComponent<NetworkInterpolation>();
        }

        /// <summary>
        ///     Server: Called when a state from client was received and update finished
        /// </summary>
        /// <param name="clientInputState"></param>
        private void ServerStateReceived(int clientInputState)
        {
            var state = new State();
            state.Frame = clientInputState;
            state.Position = transform.position;
            state.Rotation = transform.rotation;

            //Server: trigger the synchronisation due to SyncVar property
            serverLastState = state;

            //If server and client is local, bypass the sync and set state as ACKed
            if (isServer && isLocalPlayer)
                SendMessage("ServerState", state, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        ///     Server: Serialize the state over network
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="initialState"></param>
        /// <returns></returns>
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            writer.Write(serverLastState.Frame);
            writer.Write(serverLastState.Position);
            writer.Write(serverLastState.Rotation);

            return true;
        }

        /// <summary>
        ///     All Clients: Deserialize the state from network
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="initialState"></param>
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            var state = new State();

            state.Frame = reader.ReadInt32();
            state.Position = reader.ReadVector3();
            state.Rotation = reader.ReadQuaternion();

            //Client: Received a new state for the local player, treat it as an ACK and do reconciliation
            if (isLocalPlayer)
            {
                SendMessage("ServerState", state, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                //Other Clients: Received a state, treat it like a new position snapshot from authority
                if (initialState)
                {
                    //Others Clients: First state, just snap to new position
                    transform.position = state.Position;
                    transform.rotation = state.Rotation;
                }
                else if (networkInterpolation != null)
                {
                    //Others Clients: Interpolate between received positions
                    networkInterpolation.ReceiveState(state);
                }
            }
        }
    }
}