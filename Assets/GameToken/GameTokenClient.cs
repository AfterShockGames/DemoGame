using UnityEngine;
using System.Runtime.InteropServices;

namespace GameToken
{
    public class GameTokenClient
    {
        public uint NodeID { get; set; }

        [DllImport("__Internal")]
        private static extern bool TransferTo(string address, uint value, uint nodeID);
    }
}
