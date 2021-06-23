using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability
{
    // This class is only used to control two different move ability scripts
    public class MoveAbilityScript : AbilityBaseScript
    {
        // Move
        protected float agentMoveSpeed;
        protected float agentRotateSpeed;
        protected float agentAccelerateLimit;  // Set this to 0 to enable "forward only" mode.

        // Search
        protected float agentRadius;
        protected float searchStepDistance;
        protected float searchStepMaxDistance;
        protected float searchMaxRandomNumber;

        protected Vector3 destination;
        protected List<Vector3> moveBeacons = new List<Vector3>();
        protected float lastFrameSpeedAdjust = 0;
        protected Vector3 lastFrameMoveDirection = new Vector3();

        protected int pathfinderLayerMask = 1 << 11;

        public override bool UseAbility(List<object> target)
        {
            if (target.Count != 2 || target[1].GetType() != typeof(Vector3))
            {
                abilityTarget = new List<object>();
            }
            if (base.UseAbility(target))
            {
                if ((int)abilityTarget[0] == 0)
                {
                    Host.SetDestination(transform.position);
                }
                else if ((int)abilityTarget[0] == 1)
                {
                    Host.SetDestination((Vector3)abilityTarget[1]);
                }
                return true;
            }
            return false;
        }
    }
}