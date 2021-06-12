using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FixedLauncherScript : AttackSubsystemBaseScript
{
    [Header("Objects")]
    [Tooltip("GameObject used to shoot from the turret")]
    public GameObject missile;
    [Tooltip("Where should the bullet instantiate")]
    public List<Transform> missileStartPosition;
    public float missileUpwardFlyDistance;

    private GameObject fireTarget;
    private int bulletCount;

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
        bulletCount = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
        if (!Active && HP / maxHP > repairPercentRequired)
        {
            OnSubsystemRepairedAction();
        }
        if (Active)
        {
            if (timer >= coolDown / missileStartPosition.Count / Parent.AttackPower)
            {
                if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                {
                    Fire(bulletCount);
                    bulletCount++;
                    if (bulletCount == missileStartPosition.Count)
                    {
                        bulletCount = 0;
                    }
                }
                else
                {
                    DetermineFireTarget();
                }
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    protected virtual void Fire(int missileIndex)
    {
        GameObject temp = Instantiate(missile, missileStartPosition[missileIndex].position, new Quaternion());
        temp.transform.LookAt(temp.transform.position + transform.up * missileUpwardFlyDistance, temp.transform.up);
        temp.transform.position += transform.up * missileUpwardFlyDistance;
        MissileBaseScript tempScript = temp.GetComponent<MissileBaseScript>();
        tempScript.target = fireTarget;
        tempScript.from = Parent.gameObject;
        tempScript.BelongTo = BelongTo;
    }

    // Try to find a target by the order, compare angleY first, then check obstacles
    protected override void DetermineFireTarget()
    {
        if (subsystemTarget == null)
        {
            return;
        }
        if (subsystemTarget.Count == 1)
        {
            fireTarget = (GameObject)subsystemTarget[0];
        }
        List<Collider> allPossibleTargets = new List<Collider>(Physics.OverlapSphere(transform.position, lockRange));
        // Only select ship, may extend in future
        allPossibleTargets = allPossibleTargets.Where(x => x.CompareTag("Unit")).Where(x => x.GetComponent<ShipBaseScript>() != null && x.GetComponent<ShipBaseScript>().BelongTo != BelongTo).ToList();
        allPossibleTargets.Sort((x, y) => Comparer.Default.Compare(
            (x.transform.position - transform.position).sqrMagnitude, (y.transform.position - transform.position).sqrMagnitude));
        if (allPossibleTargets.Count != 0)
        {
            fireTarget = allPossibleTargets[0].gameObject;
        }
        else
        {
            fireTarget = null;
        }
    }
}
