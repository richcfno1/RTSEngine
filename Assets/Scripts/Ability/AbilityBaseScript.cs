using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Unit;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability
{
    public class AbilityBaseScript : MonoBehaviour
    {
        public enum AbilityType
        {
            None,
            Move,
            Attack,
            Carrier
        }

        // Set when instantiate
        public UnitBaseScript Host { get; set; }
        public List<SubsystemBaseScript> SupportedBy { get; set; } = new List<SubsystemBaseScript>();

        protected List<object> abilityTarget = new List<object>();

        public virtual bool UseAbility(List<object> target)
        {
            // This only happened when supported by unit itself
            if (SupportedBy.Count == 0)
            {
                abilityTarget = target;
                return true;
            }
            foreach (SubsystemBaseScript i in SupportedBy)
            {
                if (i.Active)
                {
                    abilityTarget = target;
                    return true;
                }
            }
            return false;
        }
    }
}