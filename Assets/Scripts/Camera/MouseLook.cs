using System;
using UnityEngine;

namespace DemoGame.Camera
{
    [Serializable]
    public class MouseLook
    {
        public bool clampVerticalRotation = true;
        private Quaternion m_CameraTargetRot;

        [SerializeField] private Quaternion m_CharacterTargetRot;

        public float MaximumX = 90F;
        public float MinimumX = -90F;
        public bool Smooth;
        public float SmoothTime = 5f;
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            var yRot = Input.GetAxis("Mouse X") * XSensitivity;
            var xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (Smooth)
            {
                character.rotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    SmoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    SmoothTime * Time.deltaTime);
            }
            else
            {
                character.rotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
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