using DemoGame.Player.Input;
using UnityEngine;

namespace DemoGame.Player
{
    /// <summary>
    ///     Manages character rotation according to CharacterInput
    /// </summary>
    public class Rotation : MonoBehaviour
    {
        private InputManager input;

        private void Awake()
        {
            input = GetComponent<InputManager>();
        }

        /// <summary>
        ///     Run update like classic unity's Update
        ///     We use an other method here because the calling must be controlled by CharacterNetwork
        ///     We can't use standard Update method because Unity update order is non-deterministic
        /// </summary>
        /// <param name="delta"></param>
        public void RunUpdate(float delta)
        {
            transform.rotation = Quaternion.Euler(0, input.CurrentInput.getYaw(), 0);
        }
    }
}