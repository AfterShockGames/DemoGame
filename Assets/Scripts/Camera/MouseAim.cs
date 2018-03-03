#region

using UnityEngine;

#endregion

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
        private UnityEngine.Camera _camera;
        [SerializeField] private MouseLook _mouseLook;

        public GameObject Target;

        public void RunUpdate(float delta)
        {
            _mouseLook.LookRotation(Target.transform.parent, _camera.transform);
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

                _camera = UnityEngine.Camera.main;
                _mouseLook.Init(target.transform.parent, _camera.transform);
            }
        }
    }
}