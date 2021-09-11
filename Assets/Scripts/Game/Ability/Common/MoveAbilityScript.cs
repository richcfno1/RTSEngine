using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.Network;
using MLAPI;
using MLAPI.Messaging;

namespace RTS.Game.Ability.CommonAbility
{
    public class MoveAbilityScript : CommonAbilityBaseScript
    {
        public void Move(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.Move(destination, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.MoveServerRpc(Host.Index, destination, clearQueue, addToEnd);
            }
        }

        public void LookAt(Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.LookAt(target, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.LookAtServerRpc(Host.Index, target, clearQueue, addToEnd);
            }
        }

        public void LookAtTarget(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.LookAtTarget(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.LookAtTargetServerRpc(Host.Index, targetIndex, clearQueue, addToEnd);
            }
        }

        public void Follow(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.Follow(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.FollowServerRpc(Host.Index, targetIndex, clearQueue, addToEnd);
            }
        }

        public void Follow(int targetIndex, Vector3 offset, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.Follow(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), offset, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.FollowServerRpc(Host.Index, targetIndex, offset, clearQueue, addToEnd);
            }
        }

        public void KeepInRange(int targetIndex, float upperBound, float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.KeepInRange(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), upperBound, lowerBound, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.KeepInRangeServerRpc(Host.Index, targetIndex, upperBound, lowerBound, clearQueue, addToEnd);
            }
        }

        public void KeepInRangeAndLookAt(int targetIndex, Vector3 offset, float upperBound,
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.KeepInRangeAndLookAt(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), offset, upperBound, lowerBound, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.KeepInRangeAndLookAtServerRpc(Host.Index, targetIndex, offset, upperBound, lowerBound, clearQueue, addToEnd);
            }
        }
    }
}