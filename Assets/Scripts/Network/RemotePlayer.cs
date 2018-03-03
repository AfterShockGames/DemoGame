using UnityEngine;
using UnityEngine.Networking;

namespace DemoGame.Network
{
    /// <summary>
    ///     Designate a Player
    /// </summary>
    [NetworkSettings(sendInterval = 1, channel = 0)]
    public class RemotePlayer : NetworkBehaviour
    {
        [SerializeField] private GameObject characterPrefab;

        private int connID;

        [SyncVar] public string DisplayName = "unnamed";

        private int hostID;

        private bool isInit;
        private float nextUpdate;

        [SyncVar] public short Ping = 999;

        [SyncVar] public NetworkInstanceId SpawnedCharacterID;

        private void Start()
        {
            transform.parent = PlayerManager.Instance.transform;
            if (isLocalPlayer)
                CmdSetDisplayName("SomeName");
        }

        private void Update()
        {
            //Init some values (first frame only)
            if (isServer && !isInit)
            {
                var identity = GetComponent<NetworkIdentity>();
                if (identity.connectionToClient != null)
                {
                    hostID = identity.connectionToClient.hostId;
                    connID = identity.connectionToClient.connectionId;
                    isInit = true;
                }
            }
            else
            {
                isInit = true;
            }

            //Update player ping
            if (isServer && !isLocalPlayer && Time.time > nextUpdate)
            {
                nextUpdate = Time.time + GetNetworkSendInterval();

                byte error;
                Ping = (short) NetworkTransport.GetCurrentRTT(hostID, connID, out error);
            }

            //TODO remove spawn code
            if (Input.GetKeyUp(KeyCode.K))
                CmdSpawnPlayer();
        }

        /// <summary>
        ///     Set player name
        /// </summary>
        /// <param name="name"></param>
        [Command]
        private void CmdSetDisplayName(string name)
        {
            DisplayName = name;
            gameObject.name = "Player " + name;
        }

        /// <summary>
        ///     Spawn a new character for this player
        /// </summary>
        [Command]
        public void CmdSpawnPlayer()
        {
            if (ClientScene.FindLocalObject(SpawnedCharacterID) == null)
            {
                var go = Instantiate(characterPrefab, Vector3.up, Quaternion.identity);
                NetworkServer.AddPlayerForConnection(GetComponent<NetworkIdentity>().connectionToClient, go, 1);
                SpawnedCharacterID = go.GetComponent<NetworkIdentity>().netId;
            }
            else
            {
                Debug.LogWarning("Server: Can't spawn two character for the same player");
            }
        }

        public GameObject GetCharacterObject()
        {
            if (SpawnedCharacterID == null)
                return null;

            return ClientScene.FindLocalObject(SpawnedCharacterID);
        }
    }
}