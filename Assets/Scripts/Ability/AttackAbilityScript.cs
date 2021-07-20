using System.Linq;
using System.Collections.Generic;
using RTS.RTSGameObject.Subsystem;
using UnityEngine;

namespace RTS.Ability
{
    public class AttackAbilityScript : AbilityBaseScript
    {
        public enum ActionType
        {
            SetPassive,
            SetNeutral,
            SetAggressive
        }

        public override bool CanUseAbility()
        {
            return base.CanUseAbility();
        }

        public void HandleAttackMode(ActionType action)
        {
            if (action == ActionType.SetPassive)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(null);
                }
                Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Passive;
            }
            else if (action == ActionType.SetNeutral)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(new List<object>());
                }
                Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Neutral;
            }
            else if (action == ActionType.SetAggressive)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(new List<object>());
                }
                Host.CurrentFireControlStatus = RTSGameObject.Unit.UnitBaseScript.FireControlStatus.Aggressive;
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
                Host.KeepInRangeAndHeadTo(target, (transform.position - target.transform.position).normalized, 
                    maxLockRange, minSuggestedFireDistance, false, false);
            }
            else
            {
                float minLockRange = Mathf.Infinity;
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    minLockRange = minLockRange < i.lockRange ? minLockRange : i.lockRange;
                    i.SetTarget(new List<object>() { target });
                }
                Host.KeepInRange(target, minLockRange, 0, false, false);
            }
        }
    }
}