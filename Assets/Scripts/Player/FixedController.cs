namespace DemoGame.Player
{
    public class FixedController : SuperCharacterController
    {
        private void Update()
        {
            //Disable SuperCharacterController Update
        }

        public void DoUpdate(float delta)
        {
            deltaTime = delta;
            SingleUpdate();
        }
    }
}