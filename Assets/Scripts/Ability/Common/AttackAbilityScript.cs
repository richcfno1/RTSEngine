using MLAPI;
using System.Linq;
using System.Collections.Generic;
using RTS.RTSGameObject.Subsystem;
using UnityEngine;
using System;
using MLAPI.Messaging;
using RTS.RTSGameObject;

namespace RTS.Ability.CommonAbility
{
    public class AttackAbilityScript : CommonAbilityBaseScript
    {
        public enum ActionType
        {
            SetPassive,
            SetNeutral,
            SetAggressive
        }

        private void Start()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                SetAggressiveServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPassiveServerRpc()
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                i.AllowAutoFire = false;
                i.SetTarget(null);
            }
            Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Passive;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNeutralServerRpc()
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                i.AllowAutoFire = true;
                i.SetTarget(new List<object>());
            }
            Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Neutral;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetAggressiveServerRpc()
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                i.AllowAutoFire = true;
                i.SetTarget(new List<object>());
            }
            Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Aggressive;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AttackServerRpc(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Attack(GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AttackAndMoveServerRpc(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            Host.AttackAndMove(destination, clearQueue, addToEnd);
        }

        public void HandleAttackStop()
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                i.SetTarget(new List<object>());
            }
        }

        // This is called by unit to set action queue
        public void HandleAttackAction(GameObject target)
        {
            if (target == null)
            {
                return;
            }
            // If the unit has axis weapon, then ???
            List<AxisBaseScript> allAxisWeapons = SupportedBy.OfType<AxisBaseScript>().ToList();
            if (allAxisWeapons.Count != 0)
            {
                float maxLockRange = 0;
                float minSuggestedFireDistance = Mathf.Infinity;
                foreach (AxisBaseScript i in SupportedBy)
                {
                    maxLockRange = maxLockRange > i.lockRange ? maxLockRange : i.lockRange;
                    minSuggestedFireDistance = minSuggestedFireDistance < i.suggestedFireDistance ? 
                        minSuggestedFireDistance : i.suggestedFireDistance;
                    i.SetTarget(new List<object>() { target });
                }
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    maxLockRange = maxLockRange < i.lockRange ? maxLockRange : i.lockRange;
                    i.SetTarget(new List<object>() { target });
                }
                // call follow and head to
                if (Host.MoveAbility != null)
                {
                    Host.MoveAbility.KeepInRangeAndLookAtServerRpc(target.GetComponent<RTSGameObjectBaseScript>().Index, 
                        (transform.position - target.transform.position).normalized,
                        maxLockRange, minSuggestedFireDistance, false, false);
                }
            }
            else
            {
                float minLockRange = Mathf.Infinity;
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    minLockRange = minLockRange < i.lockRange ? minLockRange : i.lockRange;
                    i.SetTarget(new List<object>() { target });
                }
                if (Host.MoveAbility != null)
                {
                    Host.MoveAbility.KeepInRangeServerRpc(target.GetComponent<RTSGameObjectBaseScript>().Index
                        , minLockRange, 0, false, false);
                }
            }
        }
    }
}