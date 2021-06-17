using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitBaseScript : RTSGameObjectBaseScript
{
    [Serializable]
    public class AnchorData
    {
        public string anchorName;
        public GameObject anchor;
        public SubsystemBaseScript.SubsystemScale subsystemScale;
        public GameObject subsystem;
    }

    // Set by editor
    [Header("Unit")]
    [Header("Subsystem")]
    [Tooltip("Subsystems, only anchor data without associated subsystem gameobject in prefab can be set in init data, or aka custome subsystem.\n" +
        "This also means fighter should not have anchor data without subsystem gameobject.")]
    public List<AnchorData> subsyetemAnchors;
    [Header("Modifier")]
    [Tooltip("Similar to HP, which can influence attack.")]
    public float maxAttackPower;
    [Tooltip("Similar to HP, which can influence defence.")]
    public float maxDefencePower;
    [Tooltip("Similar to HP, which can influence move.")]
    public float maxMovePower;

    [Tooltip("Recover rate")]
    [Range(0, 1)]
    public float recoverAttackPower;
    [Tooltip("Recover rate")]
    [Range(0, 1)]
    public float recoverDefencePower;
    [Tooltip("Recover rate")]
    [Range(0, 1)]
    public float recoverMovePower;

    [Header("Build")]
    public float buildProce;
    public float buildTime;

    // Set when instantiate
    public string UnitTypeID { get; set; }
    // Three basic value which indicate the performance of ability
    public float AttackPower { get { return curAttackPower / maxAttackPower; } set { curAttackPower = value * maxAttackPower; } }
    public float DefencePower { get { return curDefencePower / maxDefencePower; } set { curDefencePower = value * maxDefencePower; } }
    public float MovePower { get { return curMovePower / maxMovePower; } set { curMovePower = value * maxMovePower; } }
    public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

    protected float curAttackPower;
    protected float curDefencePower;
    protected float curMovePower;

    protected bool enablePathfinder = true;
    protected Vector3 destination;
    protected List<Vector3> moveBeacons = new List<Vector3>();
    protected List<Vector3> forcedMoveDestinations = new List<Vector3>();

    public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
    {
        curAttackPower = Mathf.Clamp(curAttackPower - attackPowerReduce, 0, maxAttackPower);
        curDefencePower = Mathf.Clamp(curDefencePower - defencePowerReduce, 0, maxDefencePower);
        curMovePower = Mathf.Clamp(curMovePower - movePowerReduce, 0, maxDefencePower);
        base.CreateDamage(damage / DefencePower, attackPowerReduce, defencePowerReduce, movePowerReduce, from);
    }

    public virtual void SetDestination(Vector3 destination)
    {
        enablePathfinder = true;
        List<Collider> allColliders = new List<Collider>();
        allColliders.AddRange(GetComponents<Collider>());
        allColliders.AddRange(GetComponentsInChildren<Collider>());
        allColliders.RemoveAll(x => x.gameObject.layer == 11);
        foreach (Collider i in allColliders)
        {
            i.enabled = true;
        }
        this.destination = destination;
        moveBeacons.Clear();
    }

    public virtual void ForcedMove(List<Vector3> destinations)
    {
        enablePathfinder = false;
        List<Collider> allColliders = new List<Collider>();
        allColliders.AddRange(GetComponents<Collider>());
        allColliders.AddRange(GetComponentsInChildren<Collider>());
        allColliders.RemoveAll(x => x.gameObject.layer == 11);
        foreach (Collider i in allColliders)
        {
            i.enabled = false;
        }
        forcedMoveDestinations = destinations;
    }
}