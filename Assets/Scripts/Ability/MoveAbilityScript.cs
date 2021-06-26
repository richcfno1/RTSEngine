using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability
{
    public class MoveAbilityScript : AbilityBaseScript
    {
        public enum ActionType
        {
            Stop,
            MoveTo,
            RotateTo
        }

        public bool UseAbility()
        {
            return true;
        }

        public bool UseMoveAbility(ActionType action, Vector3 target = new Vector3(), bool clearQueue = true)
        {
            if (base.CanUseAbility())
            {
                if (action == ActionType.Stop)
                {
                    Host.Stop();
                }
                else if (action == ActionType.MoveTo)
                {
                    Host.Move(target, clearQueue);
                }
                else if (action == ActionType.RotateTo)
                {
                    Host.HeadTo(target, clearQueue);
                }
                return true;
            }
            return false;
        }
    }
}