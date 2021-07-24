using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Ability.SpecialAbility
{
    public class SelectTargetSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public float distance;
        public List<string> possibleTargetTags;

        public virtual void UseAbility(GameObject target)
        {
            if (possibleTargetTags.Contains(target.tag) && (Host.transform.position - target.transform.position).magnitude <= distance)
            {
                supportedBy.Use(target);
            }
        }
    }
}

