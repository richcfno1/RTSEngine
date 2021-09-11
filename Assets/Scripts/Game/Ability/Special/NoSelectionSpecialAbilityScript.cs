using MLAPI;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.RTSGameObject.Subsystem;
using RTS.Game.Network;

namespace RTS.Game.Ability.SpecialAbility
{
    public class NoSelectionSpecialAbilityScript : SpecialAbilityBaseScript
    {
        public virtual void ParseSpecialAbility()
        {
            supportedBy.Use();
        }

        public virtual void UseAbility(bool clearQueue = true, bool addToEnd = true)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Host.UseNoSelectionSpecialAbility(this, clearQueue, addToEnd);
            }
            else
            {
                LocalPlayerScript.LocalPlayer.UseAbilityServerRpc(supportedBy.Index, clearQueue, addToEnd);
            }
        }
    }
}

