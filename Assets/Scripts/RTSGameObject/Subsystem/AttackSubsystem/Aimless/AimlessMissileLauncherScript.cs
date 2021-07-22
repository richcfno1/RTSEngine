using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.RTSGameObject.Projectile.Missile;

namespace RTS.RTSGameObject.Subsystem
{
    public class AimlessMissileLauncherScript : AimlessBaseScript
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
                if (timer >= coolDown / missileStartPosition.Count / Host.AttackPower)
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
                    DetermineFireTarget();
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
            tempScript.createdBy = Host.gameObject;
            tempScript.BelongTo = BelongTo;
        }

        // Try to find a target by the order, compare angleY first, then check obstacles
        protected override void DetermineFireTarget()
        {
            if (subsystemTarget == null)
            {
                fireTarget = null;
                return;
            }
            if (subsystemTarget.Count == 1 && (GameObject)subsystemTarget[0] != null && possibleTargetTags.Contains(((GameObject)subsystemTarget[0]).tag))
            {
                fireTarget = (GameObject)subsystemTarget[0];
                return;
            }
            List<Collider> allPossibleTargets = new List<Collider>(Physics.OverlapSphere(transform.position, lockRange, ~pathfinderLayerMask));
            List<Collider> filteredPossibleTargets = new List<Collider>();
            foreach (string i in possibleTargetTags)
            {
                filteredPossibleTargets.AddRange(allPossibleTargets.Where(x => x.CompareTag(i)));
            }
            filteredPossibleTargets = filteredPossibleTargets.Where(x => x.GetComponent<RTSGameObjectBaseScript>() != null &&
                x.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo).ToList();
            filteredPossibleTargets.Sort((x, y) => Comparer.Default.Compare(
                (x.transform.position - transform.position).sqrMagnitude, (y.transform.position - transform.position).sqrMagnitude));
            if (filteredPossibleTargets.Count != 0)
            {
                fireTarget = filteredPossibleTargets[0].gameObject;
            }
            else
            {
                fireTarget = null;
            }
        }
    }
}