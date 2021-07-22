using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.CommonAbility
{
    public class CarrierAbilityScript : CommonAbilityBaseScript
    {
        public enum ActionType
        {
            Produce,
            Deploy,
            Recall
        }

        // For action produce and deploy, second target should be string. Or target should be GameObject
        public bool UseCarrierAbility(ActionType action, object target)
        {
            // Should not use foreach because there should be only one subsystem support carrier ability?
            bool result = false;
            if (base.CanUseAbility())
            {
                if (action == ActionType.Produce)
                {
                    foreach (CarrierSubsystemBaseScript i in SupportedBy)
                    {
                        result |= i.Produce((string)target);
                    }
                }
                else if (action == ActionType.Deploy)
                {
                    foreach (CarrierSubsystemBaseScript i in SupportedBy)
                    {
                        result |= i.Deploy((string)target);
                    }
                }
                else if (action == ActionType.Recall)
                {
                    foreach (CarrierSubsystemBaseScript i in SupportedBy)
                    {
                        result |= i.Recall((GameObject)target);
                    }
                }
                return result;
            }
            return result;
        }
    }
}