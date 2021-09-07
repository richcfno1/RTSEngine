using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Network
{
    public class LocalPlayerScript : NetworkBehaviour
    {
        public static LocalPlayerScript LocalPlayer 
        {
            get
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var client))
                {
                    return client.PlayerObject.GetComponent<LocalPlayerScript>();
                }
                else
                {
                    return null;
                }
            }
        }

        public int PlayerIndex
        {
            get { return playerIndex.Value; }
            set { playerIndex.Value = value; }
        }
        private NetworkVariable<int> playerIndex = new NetworkVariable<int>(-1);

        public override void NetworkStart()
        {
            SubmitSetIndexRequestServerRpc();
        }

        [ServerRpc]
        void SubmitSetIndexRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            if (int.TryParse(RTSNetworkManager.requiredlayerIndexText, out int value))
            {
                PlayerIndex = value;
            }
            else
            {
                PlayerIndex = -1;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}