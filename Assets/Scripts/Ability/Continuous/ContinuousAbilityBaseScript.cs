using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousAbilityBaseScript : AbilityBaseScript
{
    protected bool isUsing;

    public override bool UseAbility(List<object> target)
    {
        // This only happened when supported by ship
        if (SupportedBy.Count == 0)
        {
            abilityTarget = target;
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

    protected virtual void ContinuousAction()
    {

    }
}
