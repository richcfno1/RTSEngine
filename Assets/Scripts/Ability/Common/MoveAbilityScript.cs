using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.CommonAbility
{
    public class MoveAbilityScript : CommonAbilityBaseScript
    {
        public override bool CanUseAbility()
        {
            return base.CanUseAbility();
        }

        public void Move(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Move(destination, clearQueue, addToEnd);
        }

        public void LookAt(Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            Host.LookAt(target, clearQueue, addToEnd);
        }

        public void LookAtTarget(GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            Host.LookAtTarget(target, clearQueue, addToEnd);
        }

        public void Follow(GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Follow(target, clearQueue, addToEnd);
        }

        public void Follow(GameObject target, Vector3 offset, bool clearQueue = true, bool addToEnd = true)
        {
            Host.Follow(target, offset, clearQueue, addToEnd);
        }

        public void KeepInRange(GameObject target, float upperBound, float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            Host.KeepInRange(target, upperBound, lowerBound, clearQueue, addToEnd);
        }

        public void KeepInRangeAndLookAt(GameObject target, Vector3 offset, float upperBound,
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            Host.KeepInRangeAndLookAt(target, offset, upperBound, lowerBound, clearQueue, addToEnd);
        }
    }
}