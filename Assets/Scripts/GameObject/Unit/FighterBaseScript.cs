using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBaseScript : UnitBaseScript
{
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
        AttackPower = Mathf.Clamp01(AttackPower + recoverAttackPower * Time.fixedDeltaTime);
        DefencePower = Mathf.Clamp01(DefencePower + recoverDefencePower * Time.fixedDeltaTime);
        MovePower = Mathf.Clamp01(MovePower + recoverMovePower * Time.fixedDeltaTime);
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
    }
}
