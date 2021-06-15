using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbilityScript : AbilityBaseScript
{
    public enum UseType
    {
        Pause,
        Auto,
        Specific
    }

    // Start is called before the first frame update
    void Start()
    {
        UseAbility(new List<object>() { UseType.Auto, null });
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    // For MoveAbility target size should be 2
    // target[0] = int where 0 = auto attack, 1 = attack specific target;
    // target[1] = game object to attack
    public override bool UseAbility(List<object> target)
    {
        // Invalid request
        if (target.Count != 2)
        {
            return false;
        }
        if (base.UseAbility(target))
        {
            if ((UseType)abilityTarget[0] == UseType.Pause)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(null);
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Auto)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(new List<object>());
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Specific)
            {
                foreach (AttackSubsystemBaseScript i in SupportedBy)
                {
                    i.SetTarget(new List<object>() { abilityTarget[1] });
                }
            }
            return true;
        }
        return false;
    }
}