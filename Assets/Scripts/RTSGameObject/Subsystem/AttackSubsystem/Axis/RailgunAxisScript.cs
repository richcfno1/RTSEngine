using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Projectile.Bullet;
using System.Linq;

namespace RTS.RTSGameObject.Subsystem
{
    public class RailgunAxisScript : AxisBaseScript
    {
        [Header("Objects")]
        [Tooltip("GameObject used to shoot from the turret")]
        public GameObject bullet;
        [Tooltip("Where should the bullet instantiate")]
        public List<Transform> bulletStartPosition;

        [Header("Random")]
        [Tooltip("How bullet randomly deviation when shooting")]
        public float allowedRandomAngle = 0.05f;

        private int shootCount = 0;  // This is the counter which help to indicate which elements in bulletStartPosition should be used

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
            if (!Active && HP / maxHP > repairPercentRequired)
            {
                OnSubsystemRepairedAction();
            }
            if (Active)
            {
                if (timer >= coolDown / bulletStartPosition.Count / Host.AttackPower)
                {
                    RaycastHit hit;
                    Vector3 rayPosition = transform.position;
                    Vector3 rayDirection = transform.forward;
                    if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange, ~pathfinderLayerMask))
                    {
                        if (AllowAutoFire)
                        {
                            if (possibleTargetTags.Contains(hit.collider.tag) && (
                                hit.collider.GetComponent<RTSGameObjectBaseScript>() == null ||
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
                        else
                        {
                            if (subsystemTarget != null && subsystemTarget.Count > 0)
                            {
                                if ((GameObject)subsystemTarget[0] == hit.collider.gameObject ||
                                    ((GameObject)subsystemTarget[0]).GetComponentsInParent<RTSGameObjectBaseScript>()
                                    .Contains(hit.collider.gameObject.GetComponent<RTSGameObjectBaseScript>()) ||
                                    ((GameObject)subsystemTarget[0]).GetComponentsInChildren<RTSGameObjectBaseScript>()
                                    .Contains(hit.collider.gameObject.GetComponent<RTSGameObjectBaseScript>()))
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
                    }
                    timer = 0;
                }
                else
                {
                    timer += Time.deltaTime;
                }    
            }
        }

        protected virtual void Fire(int index)
        {
            GameObject temp = Instantiate(bullet, bulletStartPosition[index].position, transform.rotation);
            BulletBaseScript tempScript = temp.GetComponent<BulletBaseScript>();
            tempScript.moveDirection = transform.forward + transform.right * Random.Range(-allowedRandomAngle, allowedRandomAngle) +
                transform.up * Random.Range(-allowedRandomAngle, allowedRandomAngle);
            tempScript.toIgnore.Add(GetComponent<Collider>());
            tempScript.toIgnore.Add(Host.GetComponent<Collider>());
            tempScript.createdBy = Host.gameObject;
        }
    }
}
