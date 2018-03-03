#region

using System;
using UnityEngine;

#endregion

namespace DemoGame.Camera
{
    [Serializable]
    public class MouseLook
    {
        private Quaternion _cameraTargetRot;

        [SerializeField] private Quaternion _characterTargetRot;
        public bool ClampVerticalRotation = true;

        public float MaximumX = 90F;
        public float MinimumX = -90F;
        public bool Smooth;
        public float SmoothTime = 5f;
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;

        public void Init(Transform character, Transform camera)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            var yRot = Input.GetAxis("Mouse X") * XSensitivity;
            var xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (ClampVerticalRotation)
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);

            if (Smooth)
            {
                character.rotation = Quaternion.Slerp(character.localRotation, _characterTargetRot,
                    SmoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, _cameraTargetRot,
                    SmoothTime * Time.deltaTime);
            }
            else
            {
                character.rotation = _characterTargetRot;
                camera.localRotation = _cameraTargetRot;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}