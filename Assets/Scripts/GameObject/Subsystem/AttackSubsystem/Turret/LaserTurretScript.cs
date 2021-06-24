using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class LaserTurretScript : TurretBaseScript
    {
        [Header("Objects")]
        [Tooltip("LineRenderer of laser")]
        public LineRenderer laserRenderer;
        [Tooltip("Where should the bullet instantiate")]
        public List<Transform> laserStartPosition;

        [Header("Laser")]
        [Tooltip("Time of renderer appear")]
        public float laserAppearTime;
        [Tooltip("Damage of single shoot")]
        public float damage;
        [Tooltip("Attack power reduced of single shoot")]
        public float attackPowerReduce;
        [Tooltip("Defence power reduced of single shoot")]
        public float defencePowerReduce;
        [Tooltip("Move power reduced of single shoot")]
        public float movePowerReduce;

        private GameObject fireTarget;
        private int shootCount;  // This is the counter which help to indicate which elements in bulletStartPosition should be used

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
            shootCount = 0;
            laserRenderer.enabled = false;
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
                if (fireTarget == null)
                {
                    SetIdle(true);
                }
                else
                {
                    SetAimpoint(fireTarget.transform.position);
                }
                RotateTurret();
                if (timer >= coolDown / laserStartPosition.Count / Host.AttackPower)
                {
                    if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                    {
                        RaycastHit hit;
                        Vector3 rayPosition = turretBarrels.position;
                        Vector3 rayDirection = turretBarrels.forward;
                        if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange, ~pathfinderLayerMask))
                        {
                            if (hit.collider.gameObject == fireTarget)
                            {
                                Fire(shootCount, fireTarget);
                                shootCount++;
                                if (shootCount == laserStartPosition.Count)
                                {
                                    shootCount = 0;
                                }
                            }
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
            if (showDebugRay && Active)
            {
                DrawDebugRay();
            }
        }

        protected virtual void Fire(int laserIndex, GameObject hit)
        {
            laserRenderer.enabled = true;
            laserRenderer.SetPositions(new Vector3[] { laserStartPosition[laserIndex].position, hit.transform.position });
            hit.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, Host.gameObject);
            StopAllCoroutines();
            StartCoroutine(HideLaser(laserAppearTime));
        }
        private IEnumerator HideLaser(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            laserRenderer.enabled = false;
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
                GameObject target = (GameObject)subsystemTarget[0];
                RaycastHit hit;
                if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit, (target.transform.position - transform.position).magnitude, ~pathfinderLayerMask))
                {
                    if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
                    {
                        fireTarget = target;
                        return;
                    }
                }
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
            foreach (Collider i in filteredPossibleTargets)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, (i.transform.position - transform.position).normalized, out hit, lockRange, ~pathfinderLayerMask))
                {
                    if (hit.collider == i)
                    {
                        fireTarget = i.gameObject;
                        return;
                    }
                }
            }
            fireTarget = null;
        }
    }
}