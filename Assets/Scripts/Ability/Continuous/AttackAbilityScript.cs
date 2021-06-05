using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbilityScript : ContinuousAbilityBaseScript
{
    // Start is called before the first frame update
    void Start()
    {
        UseAbility(new List<object>() { 1, null });
    }

    // Update is called once per frame
    void Update()
    {
        if (isUsing)
        {
            ContinuousAction();
        }
    }

    // For MoveAbility target size should be 2
    // target[0] = int where 0 = stop, 1 = auto attack, 2 = attack specific target;
    // target[1] = game object to attack
    public override bool UseAbility(List<object> target)
    {
        if (target.Count != 2)
        {
            abilityTarget = null;
            return isUsing = false;
        }
        return base.UseAbility(target);
    }

    public override void PauseAbility()
    {
        base.PauseAbility();
    }

    protected override void ContinuousAction()
    {
        if ((int)abilityTarget[0] == 1)
        {
            foreach (SubsystemBaseScript i in SupportedBy)
            {
                i.SetTarget(new List<object>() { GameObject.Find("GameObject") });
            }
        }
        
    }
}