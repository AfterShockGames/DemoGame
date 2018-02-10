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
        public float distance = 100;

        public float pitch;
        public float yaw;
        public Vector3 aimPoint;

        public void RunUpdate(float delta)
        {
            pitch = transform.rotation.eulerAngles.x;
            yaw = transform.rotation.eulerAngles.y;
            aimPoint = transform.position + transform.forward * distance;
        }
    }
}