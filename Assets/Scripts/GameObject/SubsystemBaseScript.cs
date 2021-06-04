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
    public List<string> supportedAbilityType;

    // Set when instantiate
    public ShipBaseScript Parent { get; set; }
    public bool Active { get; private set; }

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
            Action();
        }
    }

    void OnDestroy()
    {
        OnDestroyedAction();
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

    public virtual void Action()
    {

    }
}
