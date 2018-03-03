#region

using System;
using UnityEngine.Networking;

#endregion

namespace DemoGame.Network
{
    /// <summary>
    ///     Manage player list
    /// </summary>
    [NetworkSettings(channel = 0, sendInterval = 2f)]
    public class PlayerManager : NetworkBehaviour
    {
        private static PlayerManager instance;

        public static PlayerManager Instance
        {
            get { return instance ?? (instance = FindObjectOfType<PlayerManager>()); }
        }

        public RemotePlayer GetLocalPlayer()
        {
            foreach (var player in FindObjectsOfType<RemotePlayer>())
                if (player.isLocalPlayer)
                    return player;

            throw new Exception("Can't find local player");
        }
    }
}