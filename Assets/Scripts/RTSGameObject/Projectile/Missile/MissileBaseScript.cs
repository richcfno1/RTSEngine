using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject;

namespace RTS.RTSGameObject.Projectile.Missile
{
    public class MissileBaseScript : ProjectileBaseScript
    {
        // Move
        public float agentMoveSpeed;
        public float agentRotateSpeed;
        public float timeBeforeEnableCollider;

        // Search
        public float agentRadius;
        public float searchStepDistance;
        public float searchStepMaxDistance;
        public float searchMaxRandomNumber;

        // Boom!
        public float damageRadius;
        public float damageTriggerRadius;
        public float maxTime;

        public GameObject interruptEffect;
        public GameObject explosionEffect;

        private Vector3 destination;
        private List<Vector3> moveBeacons = new List<Vector3>();

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
            thisRigidbody = GetComponent<Rigidbody>();
            GetComponent<Collider>().enabled = false;
            StartCoroutine(EnableCollider(timeBeforeEnableCollider));
        }
        private IEnumerator EnableCollider(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            GetComponent<Collider>().enabled = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            // Run out of time or lose target
            if (timer >= maxTime || target == null)
            {
                OnDestroyedAction();
                return;
            }
            // Been damaged, then boom immediately (?)
            if (HP <= 0)
            {
                Instantiate(interruptEffect, transform.position, new Quaternion());
                OnDestroyedAction();
                return;
            }
            destination = target.GetComponent<Collider>().ClosestPoint(thisRigidbody.position);
            if ((thisRigidbody.position - destination).magnitude > damageTriggerRadius)
            {
                if (TestObstacle(thisRigidbody.position, destination) == 0)
                {
                    moveBeacons.Clear();
                    moveBeacons.Add(destination);
                }
                if (moveBeacons.Count != 0)
                {
                    Vector3 moveVector = moveBeacons[0] - thisRigidbody.position;
                    Vector3 rotateDirection = moveVector.normalized;
                    thisRigidbody.rotation = Quaternion.RotateTowards(thisRigidbody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime;
                    if (moveVector.magnitude <= moveDistance)
                    {
                        if (TestObstacle(thisRigidbody.position, moveBeacons[0]) != 0)
                        {
                            FindPath(thisRigidbody.position, destination);
                            return;
                        }
                        thisRigidbody.position = moveBeacons[0];
                        moveBeacons.RemoveAt(0);
                    }
                    else
                    {
                        if (TestObstacle(thisRigidbody.position, thisRigidbody.position + transform.forward * moveDistance) != 0)
                        {
                            FindPath(thisRigidbody.position, destination);
                            return;
                        }
                        thisRigidbody.position += transform.forward * moveDistance;
                    }
                }
                else
                {
                    FindPath(thisRigidbody.position, destination);
                }
            }
            else
            {
                Boom();
            }
        }

        private void Boom()
        {
            Collider[] allHits = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (Collider i in allHits)
            {
                if (!i.CompareTag("Missile") && i.GetComponent<RTSGameObjectBaseScript>() != null)
                {
                    i.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, createdBy);
                }
            }
            timer = maxTime;
            Instantiate(explosionEffect, transform.position, new Quaternion());
        }

        private float TestObstacle(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            List<Collider> toIgnore = new List<Collider>(Physics.OverlapSphere(from, agentRadius));
            toIgnore.AddRange(target.GetComponentsInChildren<Collider>());
            RaycastHit[] hits = Physics.CapsuleCastAll(from, from + direction * agentRadius * 5, agentRadius, direction, direction.magnitude);
            foreach (RaycastHit i in hits)
            {
                if (!toIgnore.Contains(i.collider) && !i.collider.CompareTag("Bullet"))
                {
                    return (i.collider.ClosestPoint(from) - from).magnitude;
                }
            }
            return 0;
        }

        private void FindPath(Vector3 from, Vector3 to)
        {
            List<Vector3> result = new List<Vector3>();
            List<Collider> intersectObjects;
            float nextStepDistance = searchStepDistance;
            bool find = false;
            result.Add(to);
            float obstacleDistance = TestObstacle(from, to);
            if (obstacleDistance != 0)
            {
                Vector3 direction = (to - from).normalized;
                Vector3 obstaclePosition = from + obstacleDistance * direction;
                Vector3 middle = new Vector3();
                nextStepDistance = searchStepDistance;
                find = false;
                while (nextStepDistance <= searchStepMaxDistance && !find)
                {
                    for (int i = 0; i < searchMaxRandomNumber; i++)
                    {
                        middle = obstaclePosition + nextStepDistance * new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1).normalized;
                        intersectObjects = new List<Collider>(Physics.OverlapSphere(middle, agentRadius));
                        intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                        if (intersectObjects.Count == 0 && TestObstacle(middle, to) == 0 && TestObstacle(from, middle) == 0)
                        {
                            find = true;
                            break;
                        }
                    }
                    nextStepDistance += searchStepDistance;
                }
                if (nextStepDistance > searchStepMaxDistance)
                {
                    Debug.Log("Out of search limitation");
                    moveBeacons.Clear();
                }
                else
                {
                    result.Insert(0, middle);
                }
            }
            moveBeacons = result;
        }
    }
}