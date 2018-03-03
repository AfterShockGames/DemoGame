#region

using DemoGame.Player.Input;
using UnityEngine;

#endregion

namespace DemoGame.Player
{
    /// <summary>
    ///     Manages caracter movement according to CharacterInput values
    /// </summary>
    public class Movement : MonoBehaviour
    {
        private int _lastGround; //Represent last tick the controller touched ground
        private Vector3 _lookDirection;
        private Vector3 _moveDirection = Vector3.zero;
        private FixedController controller;

        [SerializeField] public float Gravity = 20.0F;

        [SerializeField] public float GravityAccel = 9f;

        private InputManager input;

        [SerializeField] public float JumpHeight = 8.0F;

        [SerializeField] public float RunSpeed = 8.0F;

        [SerializeField] public float Speed = 6.0F;

        private void Awake()
        {
            input = GetComponent<InputManager>();
            controller = GetComponent<FixedController>();
            _lookDirection = transform.forward;
        }

        /// <summary>
        ///     Run update like classic unity's Update
        ///     We use an other method here because the calling must be controlled by CharacterNetwork
        ///     We can't use standard Update method because Unity update order is non-deterministic
        /// </summary>
        /// <param name="delta"></param>
        public void RunUpdate(float delta)
        {
            controller.DoUpdate(delta);
        }

        /// <summary>
        ///     Called by the controller when wanting to update custom code data
        /// </summary>
        private void SuperUpdate()
        {
            //Allways look forward
            _lookDirection = transform.forward;

            //Adjust somes values
            _lastGround++;

            if (AcquiringGround())
                _lastGround = 0;

            //Calculate movement from keys
            var actualSpeed = Speed;

            if (input.CurrentInput.Run)
                actualSpeed = RunSpeed;

            var movement = Vector3.MoveTowards(_moveDirection, LocalMovement() * actualSpeed, Mathf.Infinity);

            if (input.CurrentInput.Jump && AcquiringGround())
            {
                //Add jump velocity if jumping

                controller.DisableClamping();
                movement.y = _moveDirection.y + CalculateJumpVelocity();
            }
            else if (_lastGround > 0)
            {
                //Calculate gravity acceleration toward ground

                controller.DisableClamping();
                movement.y = _moveDirection.y - GravityAccel * controller.deltaTime;
            }
            else
            {
                controller.EnableClamping();
            }

            if (input.CurrentInput.Fire)
                Debug.Log("firing");

            _moveDirection = movement;
            controller.debugMove = _moveDirection;
        }

        /// <summary>
        ///     Constructs a vector representing our movement local to our lookDirection, which is
        ///     controlled by the camera
        /// </summary>
        private Vector3 LocalMovement()
        {
            var right = Vector3.Cross(controller.up, _lookDirection);

            var local = Vector3.zero;

            if (input.CurrentInput.HorizontalInput != 0)
                local += right * input.CurrentInput.HorizontalInput;

            if (input.CurrentInput.VerticalInput != 0)
                local += _lookDirection * input.CurrentInput.VerticalInput;

            return local.normalized;
        }


        private bool AcquiringGround()
        {
            return controller.currentGround.IsGrounded(false, 0.01f);
        }

        private bool MaintainingGround()
        {
            return controller.currentGround.IsGrounded(true, 0.5f);
        }

        private float CalculateJumpVelocity()
        {
            return Mathf.Sqrt(0.5f * JumpHeight * GravityAccel);
        }
    }
}