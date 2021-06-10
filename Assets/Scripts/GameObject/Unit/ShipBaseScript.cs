using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBaseScript : UnitBaseScript
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

    protected override void OnCreatedAction()
    {
        base.OnCreatedAction();
        AttackPower = 1;
        DefencePower = 1;
        MovePower = 1;
    }

    protected override void OnDestroyedAction()
    {
        base.OnDestroyedAction();
        foreach (AnchorData i in subsyetemAnchors)
        {
            GameManager.GameManagerInstance.OnGameObjectDestroyed(i.subsystem, lastDamagedBy);
        }
    }
}
