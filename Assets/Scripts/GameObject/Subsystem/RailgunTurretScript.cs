using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailgunTurretScript : AttackSubsystemBaseScript
{
    public GameObject bullet;
    
    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active && HP / maxHP > repairPercentRequired)
        {
            OnSubsystemRepairedAction();
        }
        if (Active)
        {
            if (timer >= coolDown)
            {
                if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                {
                    Fire();
                    timer = 0;
                }
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        
    }

    protected virtual void Fire()
    {
        Debug.Log("Fire!");
    }

    public override void SetTarget(List<object> target)
    {
        base.SetTarget(target);
        fireTarget = (GameObject) target[0];
    }
}
