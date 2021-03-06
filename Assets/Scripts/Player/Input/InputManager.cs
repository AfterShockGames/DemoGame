﻿#region

using DemoGame.Camera;
using UnityEngine;
using UnityEngine.Networking;
using UnityInput = UnityEngine.Input;

#endregion

namespace DemoGame.Player.Input
{
    /// <summary>
    ///     Fetches local input and store it
    /// </summary>
    /// <remarks>
    ///     Used as a proxy for Unity's Input class
    ///     Sometimes we need to fake some inputs (when replaying old states for example)
    /// </remarks>
    public class InputManager : NetworkBehaviour
    {
        private AimPoint cameraAim;

        public State CurrentInput;

        private void Awake()
        {
            cameraAim = UnityEngine.Camera.main.GetComponent<AimPoint>();
        }

        /// <summary>
        ///     Ask to update input from Unity's Input
        /// </summary>
        public void Parse(int inputState)
        {
            if (!isLocalPlayer)
                return;

            CurrentInput.InputState = inputState;

            CurrentInput.Left = UnityInput.GetKey(KeyCode.LeftArrow) || UnityInput.GetKey(KeyCode.A);
            CurrentInput.Right = UnityInput.GetKey(KeyCode.RightArrow) || UnityInput.GetKey(KeyCode.D);
            CurrentInput.Forward = UnityInput.GetKey(KeyCode.UpArrow) || UnityInput.GetKey(KeyCode.W);
            CurrentInput.Backward = UnityInput.GetKey(KeyCode.DownArrow) || UnityInput.GetKey(KeyCode.S);

            CurrentInput.SetPitch(cameraAim.Pitch);
            CurrentInput.SetYaw(cameraAim.Yaw);

            CurrentInput.Jump = UnityInput.GetButton("Jump");
            CurrentInput.Fire = UnityInput.GetButton("Fire1");
            CurrentInput.Aim = UnityInput.GetButton("Fire2");
            CurrentInput.Run = UnityInput.GetButton("Run");
        }
    }
}