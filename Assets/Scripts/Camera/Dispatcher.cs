#region

using UnityEngine;

#endregion

namespace DemoGame.Camera
{
    /// <summary>
    ///     Central camera for dispatching event and enabling/disabling other camera scripts
    ///     If we are in game, we activate the player camera scripts
    ///     If not, we activate spectator camera scripts
    /// </summary>
    public class Dispatcher : MonoBehaviour
    {
        private MouseAim _mouseAim;

        private void Start()
        {
            _mouseAim = GetComponent<MouseAim>();
            _mouseAim.enabled = false;
        }

        /// <summary>
        ///     Set current character target (ie: when spawning a new local player)
        /// </summary>
        /// <param name="target"></param>
        public void SetCurrentCharacterTarget(GameObject target)
        {
            _mouseAim.SetTarget(target);
        }
    }
}