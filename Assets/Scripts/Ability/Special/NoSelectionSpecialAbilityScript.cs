using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.SpecialAbility
{
    public class NoSelectionSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public virtual void ParseSpecialAbility()
        {
            supportedBy.Use();
        }

        public virtual void UseAbility(bool clearQueue = true, bool addToEnd = true)
        {
            Host.UseNoSelectionSpecialAbility(this, clearQueue, addToEnd);
        }
    }
}

