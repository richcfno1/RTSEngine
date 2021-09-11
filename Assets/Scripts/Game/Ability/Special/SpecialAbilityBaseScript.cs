using MLAPI;
using RTS.Game.RTSGameObject.Unit;
using RTS.Game.RTSGameObject.Subsystem;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Game.Ability.SpecialAbility
{
    public class SpecialAbilityBaseScript : MonoBehaviour
    {
        public string specialAbilityID;
        public Sprite specialAbilityIcon;
        public SpecialAbilitySubsystemBaseScript supportedBy;

        // Set when instantiate
        public UnitBaseScript Host { get; set; }

        public virtual bool CanUseAbility()
        {
            return supportedBy.Active;
        }

        public virtual float GetCoolDownPercent()
        {
            return supportedBy.GetCoolDownPercent();
        }
    }
}