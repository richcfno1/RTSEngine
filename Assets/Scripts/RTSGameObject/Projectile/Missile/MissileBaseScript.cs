using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject;
using RTS.Helper;
using System.Linq;

namespace RTS.RTSGameObject.Projectile.Missile
{
    public class MissileBaseScript : ProjectileBaseScript
    {
        [Header("Movement")]
        [Tooltip("Multiplier for all forces.")]
        public float forceMultiplier;
        [Tooltip("Force for move.")]
        public float maxForce;
        [Tooltip("Rotation limitation.")]
        public float maxRotationSpeed;
        [Tooltip("Slowdown distance.")]
        public float slowDownDistance;

        [Header("Pathfinder")]
        [Tooltip("The radius difference between each search sphere.")]
        public float searchStepDistance;
        [Tooltip("The max radius of search sphere.")]
        public float searchStepMaxDistance;
        [Tooltip("Pathfinder distance.")]
        public float maxDetectDistance;
        [Tooltip("Display debug path trace in game.")]
        public bool displayDebugPath;

        [Header("Explosion")]
        [Tooltip("Damage radius.")]
        public float damageRadius;
        [Tooltip("The distance to trigger the explosion.")]
        public float damageTriggerRadius;
        [Tooltip("Max fly time.")]
        public float maxTime;

        [Header("Effect")]
        [Tooltip("When missile is destroyed.")]
        public GameObject interruptEffect;
        [Tooltip("When missile is triggered.")]
        public GameObject explosionEffect;

        [HideInInspector]
        public float timeBeforeEnableCollider;

        private Vector3 finalPosition;
        private List<Vector3> moveBeacons = new List<Vector3>();
        private readonly List<Vector3> agentCorners = new List<Vector3>();
        private List<Collider> toIgnore;
        private float estimatedMaxSpeed;

        private LineRenderer debugLineRender;

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();

            thisBody = GetComponent<Rigidbody>();
            foreach (Collider i in GetComponentsInChildren<Collider>())
            {
                i.enabled = false;
            }
            StartCoroutine(EnableCollider(timeBeforeEnableCollider));

            Vector3 min = NavigationCollider.center - NavigationCollider.size * 0.5f;
            Vector3 max = NavigationCollider.center + NavigationCollider.size * 0.5f;
            agentCorners.Add(Vector3.zero);
            agentCorners.Add(new Vector3(min.x, min.y, min.z));
            agentCorners.Add(new Vector3(min.x, min.y, max.z));
            agentCorners.Add(new Vector3(min.x, max.y, min.z));
            agentCorners.Add(new Vector3(min.x, max.y, max.z));
            agentCorners.Add(new Vector3(max.x, min.y, min.z));
            agentCorners.Add(new Vector3(max.x, min.y, max.z));
            agentCorners.Add(new Vector3(max.x, max.y, min.z));
            agentCorners.Add(new Vector3(max.x, max.y, max.z));

            toIgnore = GetComponentsInChildren<Collider>().ToList();
            toIgnore.AddRange(createdBy.GetComponentsInChildren<Collider>());
            toIgnore.AddRange(target.GetComponentsInChildren<Collider>());
            estimatedMaxSpeed = ((maxForce * forceMultiplier / thisBody.drag) - Time.fixedDeltaTime * maxForce * forceMultiplier) / thisBody.mass;

            if (displayDebugPath)
            {
                debugLineRender = gameObject.AddComponent<LineRenderer>();
                debugLineRender.startWidth = debugLineRender.endWidth = 2;
            }
        }

        private IEnumerator EnableCollider(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            foreach (Collider i in GetComponentsInChildren<Collider>())
            {
                i.enabled = true;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            SetSeed();
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
            finalPosition = target.GetComponent<Collider>().ClosestPoint(thisBody.position);
            if (MoveToFinalPosition())
            {
                Explosion();
            }

            if (displayDebugPath)
            {
                Vector3[] array = new Vector3[moveBeacons.Count + 1];
                array[0] = transform.position;
                debugLineRender.positionCount = moveBeacons.Count + 1;
                for (int i = 0; i < moveBeacons.Count; i++)
                {
                    array[i + 1] = moveBeacons[i];
                }
                debugLineRender.SetPositions(array);
            }
        }

        private void Explosion()
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

        // Physical movement
        private void ApplyForce(Vector3 targetPosition, bool forwardOnly = true, bool allowSlowDown = true)
        {
            Vector3 moveVector = targetPosition - thisBody.position;
            float appliedForce = Mathf.Lerp(0, maxForce,
                allowSlowDown ? Mathf.Clamp01(moveVector.magnitude / slowDownDistance) : 1);
            if (forwardOnly)
            {
                thisBody.AddRelativeForce(0, 0, appliedForce * forceMultiplier);
            }
            else
            {
                thisBody.AddForce(moveVector.normalized * appliedForce * forceMultiplier);
            }
        }

        private void ApplyRotation(Vector3 targetPosition)
        {
            Vector3 rotateDirection = (targetPosition - thisBody.position).normalized;
            thisBody.MoveRotation(Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * maxRotationSpeed));
        }

        // Variable finalPosition must be valid
        // Return true if arrive (distance less than trigger distance)
        private bool MoveToFinalPosition()
        {
            // Try to move directly
            if (TestObstacleInPath(thisBody.position, finalPosition, maxDetectDistance) == 0)
            {
                moveBeacons.Clear();
                moveBeacons.Add(finalPosition);
            }
            // Ensure there are no obstacles in front of the unit and a valid path exist 
            if (moveBeacons.Count == 0 || TestObstacleInPath(thisBody.position, moveBeacons[0], maxDetectDistance) != 0)
            {
                FindPath(thisBody.position, finalPosition);
            }
            else
            {
                ApplyRotation(moveBeacons[0]);
                ApplyForce(moveBeacons[0]);
            }
            return (thisBody.position - finalPosition).magnitude <= damageTriggerRadius;
        }

        private float TestObstacleInPath(Vector3 from, Vector3 to, float maxDistance = Mathf.Infinity, bool considerObstacleVelocity = true)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Mathf.Min((to - from).magnitude, maxDistance);
            foreach (Vector3 i in agentCorners)
            {
                RaycastHit hit;
                if (Physics.Raycast(from - transform.position + transform.TransformPoint(i), direction, out hit, distance))
                {
                    if (!toIgnore.Contains(hit.collider) && hit.collider.GetComponentInParent<RTSGameObjectBaseScript>() != null)
                    {
                        if (objectScale <= hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().objectScale)
                        {
                            if (considerObstacleVelocity && hit.collider.GetComponentInParent<Rigidbody>() != null)
                            {
                                if (UnitVectorHelper.CollisionBetwenTwoUnitPath(from, estimatedMaxSpeed, radius, to,
                                    hit.collider.transform.position, hit.collider.GetComponentInParent<Rigidbody>().velocity,
                                    hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().radius))
                                {
                                    return (hit.collider.ClosestPoint(from) - from).magnitude;
                                }
                            }
                            else
                            {
                                return (hit.collider.ClosestPoint(from) - from).magnitude;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private void FindPath(Vector3 from, Vector3 to)
        {
            List<Vector3> result = new List<Vector3>();
            result.Add(to);
            float obstacleDistance = TestObstacleInPath(from, to, maxDetectDistance);
            if (obstacleDistance != 0)
            {
                Vector3 direction = (to - from).normalized;
                Vector3 obstaclePosition = from + obstacleDistance * direction;
                Vector3 middle = new Vector3();
                float nextStepDistance = searchStepDistance;
                bool find = false;
                while (nextStepDistance <= searchStepMaxDistance && !find)
                {
                    foreach (Vector3 i in UnitVectorHelper.GetEightSurfaceTagent(direction, nextStepDistance))
                    {
                        middle = i + obstaclePosition;
                        List<Collider> intersectObjects = new List<Collider>(Physics.OverlapBox(middle, NavigationCollider.size, transform.rotation));
                        intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                        intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
                        intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
                        if (intersectObjects.Count == 0 && TestObstacleInPath(middle, to) == 0 && TestObstacleInPath(from, middle) == 0)
                        {
                            find = true;
                            break;
                        }
                    }
                    nextStepDistance += searchStepDistance;
                }
                if (!find)
                {
                    Vector3 avoidancePosition = transform.position + transform.up * 100;
                    result.Insert(0, avoidancePosition);
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