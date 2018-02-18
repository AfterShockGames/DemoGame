using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using DemoGame.Camera;

namespace DemoGame.Player.Input
{
    /// <summary>
    /// Fetches local input and store it
    /// </summary>
    /// <remarks>
    /// Used as a proxy for Unity's Input class 
    /// Sometimes we need to fake some inputs (when replaying old states for example)
    /// </remarks>
    public class InputManager : NetworkBehaviour
    {

        private AimPoint cameraAim;

        public State currentInput;

        void Awake()
        {
            cameraAim = UnityEngine.Camera.main.GetComponent<AimPoint>();
        }

        /// <summary>
        /// Ask to update input from Unity's Input
        /// </summary>
        public void Parse(int inputState)
        {
            if (isLocalPlayer)
            {
                currentInput.inputState = inputState;

                currentInput.setInputHorizontal(UnityEngine.Input.GetAxis("Horizontal"));
                currentInput.setInputVertical(UnityEngine.Input.GetAxis("Vertical"));
                currentInput.setPitch(cameraAim.pitch);
                currentInput.setYaw(cameraAim.yaw);

                currentInput.inputJump = UnityEngine.Input.GetButton("Jump");
                currentInput.inputFire = UnityEngine.Input.GetButton("Fire1");
                currentInput.inputAim = UnityEngine.Input.GetButton("Fire2");
                currentInput.inputRun = UnityEngine.Input.GetButton("Run");
            }
        }
    }
}