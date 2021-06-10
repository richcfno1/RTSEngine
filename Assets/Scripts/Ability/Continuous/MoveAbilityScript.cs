using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is only used to control two different move ability scripts
public class MoveAbilityScript : ContinuousAbilityBaseScript
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

    public float AgentRadius { get { return agentRadius; } }

    public override bool UseAbility(List<object> target)
    {
        return base.UseAbility(target);
    }
}
