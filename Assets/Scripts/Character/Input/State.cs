using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Character.Input
{
    /// <summary>
    /// Internal an clean struct to store and transmit inout states
    /// </summary>
    /// <remarks>
    /// Maybe we don't need float precision for inputVertical or Horizontal
    /// We can store it in short or ints by multipling and rounding values.
    /// Just make sure to do it here so client and server simulate using the same rounded values
    /// </remarks>
    [System.Serializable]
    public struct State
    {
        public int inputState;

        public sbyte inputHorizontal;
        public sbyte inputVertical;
        public short pitch;
        public short yaw;

        public bool inputJump;
        public bool inputFire;
        public bool inputAim;
        public bool inputRun;

        public void setPitch(float value)
        {
            pitch = (short)(value * 10);
        }

        public void setYaw(float value)
        {
            yaw = (short)(value * 10);
        }

        public float getPitch()
        {
            return (float)pitch / 10;
        }

        public float getYaw()
        {
            return (float)yaw / 10;
        }

        public void setInputHorizontal(float value)
        {
            inputHorizontal = (sbyte)(value * 127);
        }

        public void setInputVertical(float value)
        {
            inputVertical = (sbyte)(value * 127);
        }

        public float getInputHorizontal()
        {
            return (float)inputHorizontal / 127;
        }

        public float getInputVertical()
        {
            return (float)inputVertical / 127;
        }
    }
}