using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBaseScript : GameObjectBaseScript
{
    [Serializable]
    public struct AnchorData
    {
        public string anchorName;
        public GameObject anchor;
        public SubsystemBaseScript.SubsystemScale subsystemScale;
    }
    // Set by editor
    public List<AnchorData> subsyetemAnchors;

    // Set when instantiate
    public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
    public Dictionary<GameObject, GameObject> SubsystemDictionary { get; set; } = new Dictionary<GameObject, GameObject>();
    public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
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
