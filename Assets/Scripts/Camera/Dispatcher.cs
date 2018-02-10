using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace DemoGame.Camera
{
    /// <summary>
    /// Central camera for dispatching event and enabling/disabling other camera scripts
    /// 
    /// If we are in game, we activate the player camera scripts
    /// If not, we activate spectator camera scripts
    /// </summary>
    public class Dispatcher : MonoBehaviour
    {

        private MouseAim mouseAim;

        void Start()
        {
            this.mouseAim = GetComponent<MouseAim>();
            this.mouseAim.enabled = false;
        }

        /// <summary>
        /// Set current character target (ie: when spawning a new local player)
        /// </summary>
        /// <param name="target"></param>
        public void SetCurrentCharacterTarget(GameObject target)
        {
            this.mouseAim.SetTarget(target);
        }

    }
}