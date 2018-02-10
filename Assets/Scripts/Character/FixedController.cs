using UnityEngine;
using System.Collections;

namespace DemoGame.Character
{
    public class FixedController : SuperCharacterController
    {

        void Update()
        {
            //Disable SuperCharacterController Update
        }

        public void DoUpdate(float delta)
        {
            base.deltaTime = delta;
            base.SingleUpdate();
        }

    }
}