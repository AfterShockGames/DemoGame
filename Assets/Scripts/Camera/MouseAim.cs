using UnityEngine;

namespace DemoGame.Camera
{
    /// <summary>
    ///     Simple script for mouse aim around a specific target
    /// </summary>
    /// <remarks>
    ///     In our case, the target is a little above characters's shoulder
    /// </remarks>
    public class MouseAim : MonoBehaviour
    {
        private UnityEngine.Camera m_Camera;

        [SerializeField] private MouseLook m_MouseLook;

        public GameObject Target;

        public void RunUpdate(float delta)
        {
            m_MouseLook.LookRotation(Target.transform.parent, m_Camera.transform);
        }

        public void SetTarget(GameObject target)
        {
            Target = target;

            if (target == null)
            {
                enabled = false;
            }
            else
            {
                transform.parent = target.transform.parent;
                transform.position = target.transform.position;
                transform.rotation = target.transform.rotation;
                enabled = true;

                m_Camera = UnityEngine.Camera.main;
                m_MouseLook.Init(target.transform.parent, m_Camera.transform);
            }
        }
    }
}