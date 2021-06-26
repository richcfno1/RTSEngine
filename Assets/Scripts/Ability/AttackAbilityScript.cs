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
            SetAggressive,
            Specific
        }

        // Start is called before the first frame update
        void Start()
        {
            UseAttackAbility(ActionType.SetNeutral);
        }

        // This is called by UI
        public bool UseAttackAbility(ActionType action, GameObject target = null, bool clearQueue = true)
        {
            if (base.CanUseAbility())
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
                else if (action == ActionType.Specific && target != null)
                {
                    Host.Attack(target, clearQueue);
                }
                return true;
            }
            return false;
        }

        // This is called by unit to set action queue
        public void ParseAttackAction(GameObject target)
        {
            if (target == null)
            {
                return;
            }
            // If the unit has axis weapon, then ???
            List<AxisBaseScript> allAxisWeapons = SupportedBy.OfType<AxisBaseScript>().ToList();
            if (allAxisWeapons.Count != 0)
            {
                // call follow and head to
            }
            else
            {
                // call follow
            }
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                i.SetTarget(new List<object>() { target });
            }
        }
    }
}