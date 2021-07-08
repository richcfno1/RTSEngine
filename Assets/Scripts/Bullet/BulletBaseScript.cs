using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Bullet
{
    public class BulletBaseScript : MonoBehaviour
    {
        public float damage;
        public float attackPowerReduce;
        public float defencePowerReduce;
        public float movePowerReduce;
        public float moveSpeed;
        public float maxTime;
        public bool isDestoryAtHit;

        public GameObject hitEffect;

        [HideInInspector]
        public Vector3 moveDirection;
        [HideInInspector]
        public List<Collider> toIgnore = new List<Collider>();
        [HideInInspector]
        public GameObject createdBy;

        private float timer;
        private Rigidbody thisRigidbody;

        // Start is called before the first frame update
        void Start()
        {
            thisRigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            thisRigidbody.MovePosition(thisRigidbody.position + moveDirection * Time.fixedDeltaTime * moveSpeed);
            timer += Time.fixedDeltaTime;
            if (timer > maxTime)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("AimCollider") || other.CompareTag("Bullet"))
            {
                return;
            }
            if (toIgnore.Contains(other))
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
            else if (other.CompareTag("Subsystem"))
            {
                other.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
            }
            else if (other.CompareTag("Fighter"))
            {
                other.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
            }
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, new Quaternion());
            }
            if (isDestoryAtHit)
            {
                Destroy(gameObject);
            }
        }
    }
}