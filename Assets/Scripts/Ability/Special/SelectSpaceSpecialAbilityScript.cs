using MLAPI;
using RTS.Network;
using RTS.RTSGameObject;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Ability.SpecialAbility
{
    public class SelectSpaceSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public float distance;

        public virtual void ParseSpecialAbility(Vector3 target)
        {
            if ((Host.transform.position - target).magnitude <= distance)
            {
                supportedBy.Use(target);
            }
        }

        public virtual void UseAbility(Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.UseSelectSpaceSpecialAbility(this, target, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.UseAbilityServerRpc(supportedBy.Index, target, clearQueue, addToEnd);
            }
        }
    }
}

