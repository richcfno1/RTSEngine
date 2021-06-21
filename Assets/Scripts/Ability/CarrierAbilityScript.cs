using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierAbilityScript : AbilityBaseScript
{
    public enum UseType
    {
        Produce,
        Deploy,
        Recall
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override bool UseAbility(List<object> target)
    {
        // Invalid request
        if (target.Count != 2)
        {
            return false;
        }
        // Should not use foreach because there should be only one subsystem support carrier ability?
        bool result = false;
        if (base.UseAbility(target))
        {
            if ((UseType)abilityTarget[0] == UseType.Produce)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    result |= i.Produce((string)abilityTarget[1]);
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Deploy)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    result |= i.Deploy((string)abilityTarget[1]);
                }
            }
            else if ((UseType)abilityTarget[0] == UseType.Recall)
            {
                foreach (CarrierSubsystemBaseScript i in SupportedBy)
                {
                    result |= i.Recall((GameObject)abilityTarget[1]);
                }
            }
            return result;
        }
        return result;
    }
}
