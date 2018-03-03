namespace DemoGame.Network
{
    /// <summary>
    ///     Add PlayerManagement on top of NetworkManager
    /// </summary>
    public class NetworkManager : UnityEngine.Networking.NetworkManager
    {
        private static NetworkManager instance;

        public static NetworkManager Instance
        {
            get { return instance ?? (instance = FindObjectOfType<NetworkManager>()); }
        }
    }
}