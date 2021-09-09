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
        private NetworkVariable<int> playerIndex = new NetworkVariable<int>(new NetworkVariableSettings()
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        }, -1);

        public override void NetworkStart()
        {
            SetPlayerIndexClientRpc();
        }

        [ClientRpc]
        public void SetPlayerIndexClientRpc()
        {
            if (IsOwner)
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
        }

        [ServerRpc]
        public void MoveServerRpc(int unitIndex, Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            Debug.Log($"RPC!!! MOVE! {unitIndex}:{destination}");
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.Move(destination, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void LookAtServerRpc(int unitIndex, Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            Debug.Log($"RPC!!! LOOKAT! {unitIndex}:{target}");
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.LookAt(target, clearQueue, addToEnd);
        }
    }
}