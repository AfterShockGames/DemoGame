#region

using UnityEngine;

#endregion

namespace DemoGame.Player
{
    public class NetworkInterpolation : MonoBehaviour
    {
        private readonly State[] _bufferedStates = new State[20];

        //Add an extra lag of 200 ms (4 commands back in time)
        //This way we always have a good chance to have received one of these 4 commands
        //TODO make this based on ping
        [SerializeField] private readonly float _interpolationBackTime = 0.4f;
        [SerializeField] private readonly float _updateRate = 0.33f;

        //keep at least 20 buffered state to interpolate from
        private int _bufferedStatesCount;
        private float _lastBufferedStateTime;

        /// <summary>
        ///     Called when a state is received from server
        /// </summary>
        /// <param name="newState"></param>
        public void ReceiveState(State newState)
        {
            //Other Clients: Shift buffer and store at first position
            for (var i = _bufferedStates.Length - 1; i >= 1; i--)
                _bufferedStates[i] = _bufferedStates[i - 1];

            _bufferedStates[0] = newState;
            _bufferedStatesCount = Mathf.Min(_bufferedStatesCount + 1, _bufferedStates.Length);

            //Other Clients: Check that states are in good order
            for (var i = 0; i < _bufferedStatesCount - 1; i++)
                if (_bufferedStates[i].Frame < _bufferedStates[i + 1].Frame)
                    Debug.LogWarning("Warning, State are in wrong order");

            _lastBufferedStateTime = Time.time;
        }

        // We interpolate only on other clients, not on the server, and not on the local client)
        private void Update()
        {
            //Loop all states
            for (var i = 0; i < _bufferedStatesCount; i++)
            {
                //State time is local time - state counter * interval between states
                var stateTime = _lastBufferedStateTime - i * _updateRate;

                //Find the first state that match now - interpTime (or take the last buffer entry)
                if (stateTime <= Time.time - _interpolationBackTime || i == _bufferedStatesCount - 1)
                {
                    //Get one step after and one before the time
                    var afterState = _bufferedStates[Mathf.Max(i - 1, 0)];
                    var afterStateTime = _lastBufferedStateTime - (i - 1) * _updateRate;
                    var beforeState = _bufferedStates[i];
                    var beforeStateTime = _lastBufferedStateTime - i * _updateRate;

                    // Use the time between the two slots to determine if interpolation is necessary
                    double length = afterStateTime - beforeStateTime;
                    var t = 0.0F;

                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    if (length > 0.0001)
                        t = (float) ((Time.time - _interpolationBackTime - beforeStateTime) / length);

                    //Do the actual interpolation
                    transform.position = Vector3.Lerp(beforeState.Position, afterState.Position, t);
                    transform.rotation = Quaternion.Slerp(beforeState.Rotation, afterState.Rotation, t);

                    break;
                }
            }
        }
    }
}