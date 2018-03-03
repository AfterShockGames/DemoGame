#region

using UnityEngine;

#endregion

namespace DemoGame.Player
{
    public struct State
    {
        public int Frame;
        public Vector3 Position;
        public Quaternion Rotation;

        public State(int frame, Vector3 position, Quaternion rotation)
        {
            Frame = frame;
            Position = position;
            Rotation = rotation;
        }
    }
}