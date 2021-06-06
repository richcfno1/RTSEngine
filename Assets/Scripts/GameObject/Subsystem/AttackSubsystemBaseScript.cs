using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSubsystemBaseScript : SubsystemBaseScript
{
    public float coolDown;
    public float lockRange;

    protected float timer = 0;
}
