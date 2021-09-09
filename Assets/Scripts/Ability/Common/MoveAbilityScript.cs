using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;
using MLAPI.Messaging;

namespace RTS.Ability.CommonAbility
{
    public class MoveAbilityScript : CommonAbilityBaseScript
    {
        [ServerRpc(RequireOwnership = false)]
        public void MoveServerRpc(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Move(destination, clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LookAtServerRpc(Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            Host.LookAt(target, clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LookAtTargetServerRpc(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            Host.LookAtTarget(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void FollowServerRpc(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Follow(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void FollowServerRpc(int targetIndex, Vector3 offset, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Follow(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), offset, clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KeepInRangeServerRpc(int targetIndex, float upperBound, float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            Host.KeepInRange(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), upperBound, lowerBound, clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KeepInRangeAndLookAtServerRpc(int targetIndex, Vector3 offset, float upperBound,
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            Host.KeepInRangeAndLookAt(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), offset, upperBound, lowerBound, clearQueue, addToEnd);
        }
    }
}