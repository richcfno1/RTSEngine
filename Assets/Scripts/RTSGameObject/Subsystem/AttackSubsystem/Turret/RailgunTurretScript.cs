using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RTS.RTSGameObject.Projectile.Bullet;

namespace RTS.RTSGameObject.Subsystem
{
    // Rotation code is written by another author: https://github.com/brihernandez/GunTurrets
    public class RailgunTurretScript : TurretBaseScript
    {
        [Header("Objects")]
        [Tooltip("GameObject used to shoot from the turret")]
        public GameObject bullet;
        [Tooltip("Where should the bullet instantiate")]
        public List<Transform> bulletStartPosition;

        [Header("Random")]
        [Tooltip("How bullet randomly deviation when shooting")]
        public float allowedRandomAngle = 0.05f;

        private GameObject fireTarget;
        private int shootCount = 0;  // This is the counter which help to indicate which elements in bulletStartPosition should be used

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
                if (timer >= coolDown / bulletStartPosition.Count / Host.AttackPower)
                {
                    if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                    {
                        RaycastHit hit;
                        Vector3 rayPosition = turretBarrels.position;
                        Vector3 rayDirection = turretBarrels.forward;
                        if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange, ~pathfinderLayerMask))
                        {
                            if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || 
                                hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
                            {
                                Fire(shootCount);
                                shootCount++;
                                if (shootCount == bulletStartPosition.Count)
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

        protected virtual void Fire(int index)
        {
            GameObject temp = Instantiate(bullet, bulletStartPosition[index].position, turretBarrels.rotation);
            BulletBaseScript tempScript = temp.GetComponent<BulletBaseScript>();
            tempScript.moveDirection = turretBarrels.forward + turretBarrels.right * Random.Range(-allowedRandomAngle, allowedRandomAngle) +
                turretBarrels.up * Random.Range(-allowedRandomAngle, allowedRandomAngle);
            tempScript.toIgnore.Add(GetComponent<Collider>());
            tempScript.toIgnore.Add(Host.GetComponent<Collider>());
            tempScript.createdBy = Host.gameObject;
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
                if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit, lockRange, ~pathfinderLayerMask))
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
                    if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
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