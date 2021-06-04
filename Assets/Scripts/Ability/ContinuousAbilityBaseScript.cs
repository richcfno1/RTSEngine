using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousAbilityBaseScript : AbilityBaseScript
{
    protected bool isUsing;
    protected object abilityTarget;
    // Start is called before the first frame update
    void Start()
    {
        isUsing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUsing)
        {
            ContinuousAction(abilityTarget);
        }
    }

    public override bool UseAbility(object target)
    {
        // This only happened when supported by ship
        if (SupportedBy.Count == 0)
        {
            return isUsing = true;
        }
        foreach (SubsystemBaseScript i in SupportedBy)
        {
            if (i.Active)
            {
                abilityTarget = target;
                return isUsing = true;
            }
        }
        abilityTarget = null;
        return isUsing = false;
    }

    public virtual void PauseAbility()
    {
        abilityTarget = null;
        isUsing = false;
    }

    public virtual void ContinuousAction(object target)
    {

    }
}
