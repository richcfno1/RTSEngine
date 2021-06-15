using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierAbilityScript : AbilityBaseScript
{
    public enum UseType
    {
        Produce,
        Deploy,
        Retrieve
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseAbility(new List<object>() { UseType.Produce, "ScopeDrone" });
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseAbility(new List<object>() { UseType.Deploy, "ScopeDrone" });
        }
    }

    public override bool UseAbility(List<object> target)
    {
        // Invalid request
        if (target.Count != 2)
        {
            return false;
        }
        // Should not use foreach because there should be only one subsystem support carrier ability
        if (base.UseAbility(target))
        {
            if ((UseType)abilityTarget[0] == UseType.Produce)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    i.Produce((string)abilityTarget[1]);
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Deploy)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    i.Deploy((string)abilityTarget[1]);
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Retrieve)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    i.Retrieve((GameObject)abilityTarget[1]);
                }
            }
            return true;
        }
        return false;
    }
}
