using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Subsystem;

namespace RTS.RTSGameObject.Projectile.Bullet
{
    public class BulletBaseScript : ProjectileBaseScript
    {
        public float moveSpeed;
        public float maxTime;
        public bool isDestoryAtHit;

        public GameObject hitEffect;

        [HideInInspector]
        public Vector3 moveDirection;
        [HideInInspector]
        public List<Collider> toIgnore = new List<Collider>();

        private Vector3 lastPosition;


        // Start is called before the first frame update
        void Start()
        {
            thisRigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // There is not a HP test, hence even if bullet is damaged, it won't be destroyed.
            timer += Time.fixedDeltaTime;
            if (timer > maxTime)
            {
                OnDestroyedAction();
            }
            lastPosition = thisRigidbody.position;
            thisRigidbody.MovePosition(thisRigidbody.position + moveDirection * Time.fixedDeltaTime * moveSpeed);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("AimCollider") || other.CompareTag("Bullet") || toIgnore.Contains(other))
            {
                return;
            }
            if (other.CompareTag("Ship"))
            {
                // If there is a subsystem gameobject in front of the bullet, hit it instead of the ship
                RaycastHit hit;
                if (Physics.Raycast(other.ClosestPoint(transform.position), moveDirection, out hit, other.bounds.size.sqrMagnitude))
                {
                    if (hit.collider != other && hit.collider.GetComponent<SubsystemBaseScript>() != null)
                    {
                        hit.collider.GetComponent<SubsystemBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
                        Instantiate(hitEffect, transform.position, new Quaternion());
                        Destroy(gameObject);
                        return;
                    }
                }
                other.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
            }
            else if (other.GetComponent<RTSGameObjectBaseScript>() != null)
            {
                other.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
            }
            if (hitEffect != null)
            {
                
                Instantiate(hitEffect, other.ClosestPoint(lastPosition), new Quaternion());
            }
            if (isDestoryAtHit)
            {
                OnDestroyedAction();
            }
        }
    }
}