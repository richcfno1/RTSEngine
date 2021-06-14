using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSubsystemBaseScript : SubsystemBaseScript
{
    public float coolDown;
    public float lockRange;
    public List<string> possibleTargetTags;

    protected int pathfinderLayerMask = 1 << 11;
    protected float timer = 0;

    public override void SetTarget(List<object> target)
    {
        base.SetTarget(target);
        DetermineFireTarget();
    }

    protected virtual void DetermineFireTarget()
    {

    }
}
