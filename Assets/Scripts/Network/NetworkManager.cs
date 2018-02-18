using UnityEngine.Networking;

namespace DemoGame.Network
{
    /// <summary>
    /// Add PlayerManagement on top of NetworkManager
    /// </summary>
    public class NetworkManager : UnityEngine.Networking.NetworkManager
    {

        private static NetworkManager instance;
        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NetworkManager>();
                    //instance.useWebSockets = true;
                }
                return instance;
            }
        }

    }
}