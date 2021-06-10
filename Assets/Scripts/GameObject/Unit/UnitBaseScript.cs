using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBaseScript : RTSGameObjectBaseScript
{
    // Set by editor
    public float maxAttackPower;
    public float maxDefencePower;
    public float maxMovePower;
    // These should be value between 0 and 1 for revover per second;
    public float recoverAttackPower;
    public float recoverDefencePower;
    public float recoverMovePower;

    // Set when instantiate
    // Three basic value which indicate the performance of ability
    public float AttackPower { get { return curAttackPower / maxAttackPower; } set { curAttackPower = Mathf.Clamp01(value) * maxAttackPower; } }
    public float DefencePower { get { return curDefencePower / maxDefencePower; } set { curDefencePower = Mathf.Clamp01(value) * maxDefencePower; } }
    public float MovePower { get { return curMovePower / maxMovePower; } set { curMovePower = Mathf.Clamp01(value) * maxMovePower; } }
    public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

    protected float curAttackPower;
    protected float curDefencePower;
    protected float curMovePower;

    public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
    {
        curAttackPower = Mathf.Clamp(curAttackPower - attackPowerReduce, 0, maxAttackPower);
        curDefencePower = Mathf.Clamp(curDefencePower - defencePowerReduce, 0, maxDefencePower);
        curMovePower = Mathf.Clamp(curMovePower - movePowerReduce, 0, maxDefencePower);
        base.CreateDamage(damage / DefencePower, attackPowerReduce, defencePowerReduce, movePowerReduce, from);
    }

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
