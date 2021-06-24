using System.Collections.Generic;
using RTS.RTSGameObject.Subsystem;
using UnityEngine;

namespace RTS.Ability
{
    public class AttackAbilityScript : AbilityBaseScript
    {
        public enum ActionType
        {
            Stop,
            Auto,
            Specific
        }

        // Start is called before the first frame update
        void Start()
        {
            UseAttackAbility(ActionType.Auto);
        }

        // For MoveAbility target size should be 2
        // target[0] = int where 0 = auto attack, 1 = attack specific target;
        // target[1] = game object to attack
        public bool UseAttackAbility(ActionType action, GameObject target = null)
        {
            if (base.CanUseAbility())
            {
                if (action == ActionType.Stop)
                {
                    foreach (AttackSubsystemBaseScript i in SupportedBy)
                    {
                        i.SetTarget(null);
                    }
                }
                else if (action == ActionType.Auto)
                {
                    foreach (AttackSubsystemBaseScript i in SupportedBy)
                    {
                        i.SetTarget(new List<object>());
                    }
                }
                else if (action == ActionType.Specific)
                {
                    foreach (AttackSubsystemBaseScript i in SupportedBy)
                    {
                        i.SetTarget(new List<object>() { abilityTarget[1] });
                    }
                }
                return true;
            }
            return false;
        }
    }
}