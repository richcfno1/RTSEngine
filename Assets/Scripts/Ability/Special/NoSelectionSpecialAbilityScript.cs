using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.SpecialAbility
{
    public class NoSelectionSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public virtual void UseAbility()
        {
            supportedBy.Use();
        }
    }
}

