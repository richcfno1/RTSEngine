using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbilityScript : ContinuousAbilityBaseScript
{
    public enum UseType
    {
        Auto,
        Specific
    }
    // Start is called before the first frame update
    void Start()
    {
        UseAbility(new List<object>() { UseType.Auto, null });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isUsing)
        {
            ContinuousAction();
        }
    }

    // For MoveAbility target size should be 2
    // target[0] = int where 0 = auto attack, 1 = attack specific target;
    // target[1] = game object to attack
    public override bool UseAbility(List<object> target)
    {
        if (target.Count != 2)
        {
            abilityTarget = null;
            return isUsing = false;
        }
        return base.UseAbility(target);
    }

    public override void PauseAbility()
    {
        base.PauseAbility();
        foreach (AttackSubsystemBaseScript i in SupportedBy)
        {
            i.SetTarget(new List<object>());
        }
    }

    protected override void ContinuousAction()
    {
        // DEBUG
        if ((UseType)abilityTarget[0] == UseType.Auto)
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                List<GameObject> temp = GameManager.GameManagerInstance.GetAllGameObjects().FindAll(x => x.GetComponent<GameObjectBaseScript>().BelongTo != Parent.BelongTo);
                temp.Sort((x, y) => (x.transform.position - transform.position).magnitude.CompareTo((y.transform.position - transform.position).magnitude));
                i.SetTarget(new List<object>(temp));
            }
        }
        else if ((UseType)abilityTarget[0] == UseType.Specific)
        {
            foreach (AttackSubsystemBaseScript i in SupportedBy)
            {
                List<GameObject> temp = GameManager.GameManagerInstance.GetAllGameObjects().FindAll(x => x.GetComponent<GameObjectBaseScript>().BelongTo != Parent.BelongTo);
                temp.Sort((x, y) => (x.transform.position - transform.position).magnitude.CompareTo((y.transform.position - transform.position).magnitude));
                temp.Insert(0, (GameObject)abilityTarget[1]);
                i.SetTarget(new List<object>(temp));
            }
        }
    }
}