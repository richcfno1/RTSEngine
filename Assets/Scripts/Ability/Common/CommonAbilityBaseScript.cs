using MLAPI;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Unit;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.CommonAbility
{
    public class CommonAbilityBaseScript : MonoBehaviour
    {
        public enum CommonAbilityType
        {
            None,
            Move,
            Attack,
            Carrier
        }

        // Set when instantiate
        public UnitBaseScript Host { get; set; }
        public List<SubsystemBaseScript> SupportedBy { get; set; } = new List<SubsystemBaseScript>();

        public virtual bool CanUseAbility()
        {
            // This only happened when supported by unit itself
            if (SupportedBy.Count == 0)
            {
                return true;
            }
            foreach (SubsystemBaseScript i in SupportedBy)
            {
                if (i.Active)
                {
                    return true;
                }
            }
            return false;
        }
    }
}