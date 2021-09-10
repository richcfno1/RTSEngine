using MLAPI;
using RTS.Network;
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

        public virtual void ParseSpecialAbility(GameObject target)
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

        public virtual void UseAbility(int targetIndex, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.UseSelectTargetSpecialAbility(this, GameManager.GameManagerInstance.GetGameObjectByIndex(targetIndex), clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.UseAbilityServerRpc(supportedBy.Index, targetIndex, clearQueue, addToEnd);
            }
        }
    }
}

