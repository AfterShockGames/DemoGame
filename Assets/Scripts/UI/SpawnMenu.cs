using DemoGame.Network;
using UnityEngine;

namespace DemoGame.UI
{
    /// <summary>
    ///     A simple component that show a demo UI
    /// </summary>
    public class SpawnMenu : MonoBehaviour
    {
        private readonly string instructionsOffline =
            "Start two game instances: \n Start one as an host \n Start one or more as a client.\n";

        private readonly string instructionsOnMovingClient =
            "On the client that move, there is \n a client side prediction (move instantly after keypress).\n";

        private readonly string instructionsOnOtherClient =
            "On others clients, a delay is added \n to allow for interpolation to work smoothly\n";

        private readonly string instructionsOnServer =
            "Start a client and spawn it to see it. \n On the server, there is no lag compensation\n (to show the laggy movement)\n";

        private readonly string instructionsSpawn = "Click on spawn to spawn a new player\n";

        private void OnGUI()
        {
            if (!NetworkManager.Instance.isNetworkActive)
            {
                //Display help
                GUI.Box(new Rect(Screen.width / 2 - 150, 80, 300, 60), instructionsOffline);
            }
            else
            {
                var player = PlayerManager.Instance.GetLocalPlayer();
                if (!player.isServer && player.GetCharacterObject() == null)
                {
                    if (GUI.Button(new Rect(10, 120, 200, 20), "Spawn")) player.CmdSpawnPlayer();
                    GUI.Box(new Rect(Screen.width / 2 - 150, 70, 300, 40), instructionsSpawn);
                }

                if (player.isServer)
                    GUI.Box(new Rect(Screen.width / 2 - 150, 120, 300, 40), instructionsOnServer);
                else
                    GUI.Box(new Rect(Screen.width / 2 - 150, 120, 300, 80),
                        instructionsOnMovingClient + instructionsOnOtherClient);
            }
        }
    }
}