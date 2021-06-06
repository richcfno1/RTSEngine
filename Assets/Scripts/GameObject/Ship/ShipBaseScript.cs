using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBaseScript : GameObjectBaseScript
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
    public List<AnchorData> subsyetemAnchors;

    // Set when instantiate
    public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
    }

    protected override void OnDestroyedAction()
    {
        base.OnDestroyedAction();
        foreach (AnchorData i in subsyetemAnchors)
        {
            GameManager.GameManagerInstance.OnGameObjectDestroyed(i.subsystem, lastDamagedBy);
        }
    }

    public bool Command(string commandType, List<object> target)
    {
        if (AbilityDictionary.ContainsKey(commandType))
        {
            AbilityDictionary[commandType].UseAbility(target);
            return false;
        }
        return true;
    }
}
