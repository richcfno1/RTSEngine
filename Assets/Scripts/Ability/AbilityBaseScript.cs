using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBaseScript : MonoBehaviour
{
    public enum AbilityType
    {
        None,
        Move,
        Attack
    }

    // Set when instantiate
    public ShipBaseScript Parent { get; set; }
    public List<SubsystemBaseScript> SupportedBy { get; set; } = new List<SubsystemBaseScript>();

    protected List<object> abilityTarget = new List<object>();

    public virtual bool UseAbility(List<object> target)
    {
        // This only happened when supported by ship
        if (SupportedBy.Count == 0)
        {
            return true;
        }
        foreach (SubsystemBaseScript i in SupportedBy)
        {
            if (i.Active)
            {
                return true;
            }
        }
        return false;
    }
}
