using RTS.RTSGameObject;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Ability.SpecialAbility
{
    public class SelectTargetSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public float distance;
        public List<string> possibleTargetTags;
        public List<GameManager.PlayerRelation> possibleRelations;

        public virtual void UseAbility(GameObject target)
        {
            if (possibleTargetTags.Contains(target.tag) && (Host.transform.position - target.transform.position).magnitude <= distance)
            {
                if (possibleRelations.Contains(GameManager.GameManagerInstance.GetPlayerRelation(
                    Host.GetComponent<RTSGameObjectBaseScript>().BelongTo, target.GetComponent<RTSGameObjectBaseScript>().BelongTo)))
                {
                    supportedBy.Use(target);
                }
            }
        }
    }
}

