#region

using System;

#endregion

namespace DemoGame.Player.Input
{
    /// <summary>
    ///     Internal an clean struct to store and transmit inout states
    /// </summary>
    /// <remarks>
    ///     Maybe we don't need float precision for inputVertical or Horizontal
    ///     We can store it in short or ints by multipling and rounding values.
    ///     Just make sure to do it here so client and server simulate using the same rounded values
    /// </remarks>
    [Serializable]
    public struct State
    {
        public int InputState;

        public bool Left;
        public bool Right;
        public bool Forward;
        public bool Backward;
        public short Pitch;
        public short Yaw;

        public bool Jump;
        public bool Fire;
        public bool Aim;
        public bool Run;

        public float HorizontalInput
        {
            get
            {
                var horizontalInput = 0f;

                if (Left)
                    horizontalInput += -1.0f;
                if (Right)
                    horizontalInput += 1.0f;

                return horizontalInput;
            }
        }

        public float VerticalInput
        {
            get
            {
                var verticalInput = 0f;

                if (Backward)
                    verticalInput += -1.0f;
                if (Forward)
                    verticalInput += 1.0f;

                return verticalInput;
            }
        }

        public void SetPitch(float value)
        {
            Pitch = (short) (value * 10);
        }

        public void SetYaw(float value)
        {
            Yaw = (short) (value * 10);
        }

        public float GetPitch()
        {
            return (float) Pitch / 10;
        }

        public float GetYaw()
        {
            return (float) Yaw / 10;
        }
    }
}