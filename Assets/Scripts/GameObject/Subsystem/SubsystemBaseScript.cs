using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubsystemBaseScript : RTSGameObjectBaseScript
{
    public enum SubsystemScale
    {
        None,
        WeaponS,
        WeaponM,
        WeaponL
    }
    // Set by editor
    public SubsystemScale scale;
    public float repairPercentRequired;
    public List<AbilityBaseScript.AbilityType> supportedAbility;

    // Set when instantiate
    public ShipBaseScript Host { get; set; }
    public bool Active { get; private set; }

    protected List<object> subsystemTarget = new List<object>();

    protected override void OnCreatedAction()
    {
        base.OnCreatedAction();
        Active = true;
    }

    protected override void OnDestroyedAction()
    {
        Active = false;
    }

    protected virtual void OnSubsystemRepairedAction()
    {
        Active = true;
    }

    public virtual void SetTarget(List<object> target)
    {
        subsystemTarget = target;
    }

    public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float powerPowerReduce, GameObject from)
    {
        Host.CreateDamage(HP == 0 ? damage : 0, attackPowerReduce, defencePowerReduce, powerPowerReduce, from);
        base.CreateDamage(damage / Host.DefencePower, attackPowerReduce, defencePowerReduce, powerPowerReduce, from);
    }
}
