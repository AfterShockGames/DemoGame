using UnityEngine;
using System.Collections;

namespace DemoGame.Camera
{
    /// <summary>
    /// Simple script for mouse aim around a specific target
    /// </summary>
    /// <remarks>
    /// In our case, the target is a little above characters's shoulder
    /// </remarks>

    public class MouseAim : MonoBehaviour
    {

        public GameObject target;

        [SerializeField] private MouseLook m_MouseLook;
        private UnityEngine.Camera m_Camera;

        public void RunUpdate(float delta)
        {
            m_MouseLook.LookRotation(target.transform.parent, m_Camera.transform);
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;

            if (target == null)
            {
                this.enabled = false;
            }
            else
            {
                this.transform.parent = target.transform.parent;
                this.transform.position = target.transform.position;
                this.transform.rotation = target.transform.rotation;
                this.enabled = true;

                m_Camera = UnityEngine.Camera.main;
                m_MouseLook.Init(target.transform.parent, m_Camera.transform);
            }
        }
    }
}