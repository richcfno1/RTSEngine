using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubsystemBaseScript : GameObjectBaseScript
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
    public List<string> supportedAbility;  // This will be used in future

    // Set when instantiate
    public ShipBaseScript Parent { get; set; }
    public bool Active { get; private set; }

    protected List<object> subsystemTarget = new List<object>();

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active && HP / maxHP > repairPercentRequired)
        {
            OnSubsystemRepairedAction();
        }
        if (Active)
        {
            // Do something here
        }
    }

    public override void OnCreatedAction()
    {
        base.OnCreatedAction();
        Active = true;
    }

    public override void OnDestroyedAction()
    {
        Active = false;
    }

    public virtual void OnSubsystemRepairedAction()
    {
        Active = true;
    }

    public virtual void SetTarget(List<object> target)
    {
        subsystemTarget = target;
    }

    public override void CreateDamage(float amount, GameObject from)
    {
        base.CreateDamage(amount, from);
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
    }
}
