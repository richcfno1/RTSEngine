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
            RotateTo,
            ForcedMoveTo,
        }

        public bool UseAbility()
        {
            return true;
        }

        public bool UseMoveAbility(ActionType action, Vector3 target = new Vector3(), bool clearQueue = true)
        {
            if (base.CanUseAbility())
            {
                if (clearQueue)
                {
                    Host.Stop();
                }

                if (action == ActionType.Stop)
                {
                    Host.Stop();
                }
                else if (action == ActionType.MoveTo)
                {
                    Host.MoveTo(target);
                }
                else if (action == ActionType.RotateTo)
                {
                    Host.RotateTo(target);
                }
                else if (action == ActionType.ForcedMoveTo)
                {
                    Host.ForcedMoveTo(target);
                }
                return true;
            }
            return false;
        }
    }
}