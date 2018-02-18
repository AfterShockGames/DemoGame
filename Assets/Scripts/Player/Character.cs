using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using DemoGame.Camera;

namespace DemoGame.Player
{
    /// <summary>
    /// Central Player script for simple behavior common to all characters
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class Character : NetworkBehaviour
    {

        [SerializeField]
        public GameObject cameraPointer;

        void Start()
        {
            GetComponent<Movement>().enabled = false;
            GetComponent<Rotation>().enabled = false;
            if (IsLocalPlayer)
            {
                //Make the camera start following this character
                UnityEngine.Camera.main.GetComponent<Dispatcher>().SetCurrentCharacterTarget(cameraPointer);
                //GetComponent<CharacterMovement>().enabled = true;
                //GetComponent<CharacterRotation>().enabled = true;
            }

            if (isServer)
            {
                GetComponent<Movement>().enabled = true;
                GetComponent<Rotation>().enabled = true;
            }
        }

        public bool IsLocalPlayer
        {
            get
            {
                return GetComponent<NetworkIdentity>().isLocalPlayer;
            }
        }

    }
}