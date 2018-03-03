using DemoGame.Camera;
using UnityEngine;
using UnityEngine.Networking;

namespace DemoGame.Player
{
    /// <summary>
    ///     Central Player script for simple behavior common to all characters
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class Character : NetworkBehaviour
    {
        [SerializeField] public GameObject CameraPointer;

        public bool IsLocalPlayer
        {
            get { return GetComponent<NetworkIdentity>().isLocalPlayer; }
        }

        private void Start()
        {
            GetComponent<Movement>().enabled = false;
            GetComponent<Rotation>().enabled = false;
            if (IsLocalPlayer)
                UnityEngine.Camera.main.GetComponent<Dispatcher>().SetCurrentCharacterTarget(CameraPointer);

            if (isServer)
            {
                GetComponent<Movement>().enabled = true;
                GetComponent<Rotation>().enabled = true;
            }
        }
    }
}