using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using RTS.Game.Ability.SpecialAbility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Game.Network
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

        // Attack Ability RPC
        [ServerRpc]
        public void SetPassiveServerRpc(int unitIndex)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).AttackAbility.SetPassive();
        }

        [ServerRpc]
        public void SetNeutralServerRpc(int unitIndex)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).AttackAbility.SetNeutral();
        }

        [ServerRpc]
        public void SetAggressiveServerRpc(int unitIndex)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).AttackAbility.SetAggressive();
        }

        [ServerRpc]
        public void AttackServerRpc(int unitIndex, int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).AttackAbility.Attack(targetIndex, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void AttackAndMoveServerRpc(int unitIndex, Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).AttackAbility.AttackAndMove(destination, clearQueue, addToEnd);
        }

        // Move Ability RPC
        [ServerRpc]
        public void MoveServerRpc(int unitIndex, Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.Move(destination, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void LookAtServerRpc(int unitIndex, Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.LookAt(target, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void LookAtTargetServerRpc(int unitIndex, int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.LookAtTarget(targetIndex, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void FollowServerRpc(int unitIndex, int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.Follow(targetIndex, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void FollowServerRpc(int unitIndex, int targetIndex, Vector3 offset, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.Follow(targetIndex, offset, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void KeepInRangeServerRpc(int unitIndex, int targetIndex, float upperBound, float lowerBound, 
            bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.KeepInRange(targetIndex, 
                upperBound, lowerBound, clearQueue, addToEnd);
        }

        [ServerRpc]
        public void KeepInRangeAndLookAtServerRpc(int unitIndex, int targetIndex, Vector3 offset, float upperBound,
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            GameManager.GameManagerInstance.GetUnitByIndex(unitIndex).MoveAbility.KeepInRangeAndLookAt(targetIndex, 
                offset, upperBound, lowerBound, clearQueue, addToEnd);
        }

        // Special Ability RPC
        [ServerRpc]
        public void UseAbilityServerRpc(int subsystemIndex, bool clearQueue = true, bool addToEnd = true)
        {
            NoSelectionSpecialAbilityScript ability = GameManager.GameManagerInstance.GetGameObjectByIndex(subsystemIndex).
                GetComponent<NoSelectionSpecialAbilityScript>();
            if (ability != null)
            {
                ability.UseAbility(clearQueue, addToEnd);
            }
        }

        [ServerRpc]
        public void UseAbilityServerRpc(int subsystemIndex, Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            SelectSpaceSpecialAbilityScript ability = GameManager.GameManagerInstance.GetGameObjectByIndex(subsystemIndex).
                GetComponent<SelectSpaceSpecialAbilityScript>();
            if (ability != null)
            {
                ability.UseAbility(target, clearQueue, addToEnd);
            }
        }

        [ServerRpc]
        public void UseAbilityServerRpc(int subsystemIndex, int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            SelectTargetSpecialAbilityScript ability = GameManager.GameManagerInstance.GetGameObjectByIndex(subsystemIndex).
                GetComponent<SelectTargetSpecialAbilityScript>();
            if (ability != null)
            {
                ability.UseAbility(targetIndex, clearQueue, addToEnd);
            }
        }
    }
}