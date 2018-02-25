using UnityEngine;
using System.Collections;

namespace DemoGame.Camera
{
    /// <summary>
    /// Calculates aimPoint based on camera current looking direction
    /// </summary>
    public class AimPoint : MonoBehaviour
    {

        [SerializeField]
        public float Distance = 100;

        public float Pitch;
        public float Yaw;
        public Vector3 Point;

        public void RunUpdate(float delta)
        {
            Pitch = transform.rotation.eulerAngles.x;
            Yaw = transform.rotation.eulerAngles.y;
            Point = transform.position + transform.forward * Distance;
        }
    }
}