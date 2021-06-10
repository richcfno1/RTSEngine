using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBaseScript : RTSGameObjectBaseScript
{
    // Set by editor
    public float maxAttackPower;
    public float maxDefencePower;
    public float maxMovePower;

    // Set when instantiate
    // Three basic value which indicate the performance of ability
    public float AttackPower { get { return curAttackPower / maxAttackPower; } set { curAttackPower = value * maxAttackPower; } }
    public float DefencePower { get { return curDefencePower / maxDefencePower; } set { curDefencePower = value * maxDefencePower; } }
    public float MovePower { get { return curMovePower / maxMovePower; } set { curMovePower = value * maxMovePower; } }
    public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

    private float curAttackPower;
    private float curDefencePower;
    private float curMovePower;

    public virtual bool Command(string commandType, List<object> target)
    {
        if (AbilityDictionary.ContainsKey(commandType))
        {
            AbilityDictionary[commandType].UseAbility(target);
            return false;
        }
        return true;
    }
}
