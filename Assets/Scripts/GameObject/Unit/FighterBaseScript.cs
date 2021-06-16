using System.Collections.Generic;
using UnityEngine;

public class FighterBaseScript : UnitBaseScript
{
    [Header("Move")]
    [Tooltip("Move speed.")]
    public float agentMoveSpeed;
    [Tooltip("Rotate speed.")]
    public float agentRotateSpeed;

    [Header("Path finder")]
    [Tooltip("The radius difference between each search sphere.")]
    public float searchStepDistance;
    [Tooltip("The max radius of search sphere.")]
    public float searchStepMaxDistance;
    [Tooltip("The number of points tested in each sphere.")]
    public float searchMaxRandomNumber;

    private float agentRadius;
    private List<Vector3> moveBeacons = new List<Vector3>();
    private float slowDownRadius;
    private Rigidbody thisBody;

    private int pathfinderLayerMask = 1 << 11;

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
        agentRadius = NavigationCollider.radius;
        destination = transform.position;
        slowDownRadius = 360 / agentRotateSpeed * agentMoveSpeed / Mathf.PI / 2;
        thisBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
        AttackPower = Mathf.Clamp01(AttackPower + recoverAttackPower * Time.fixedDeltaTime);
        DefencePower = Mathf.Clamp01(DefencePower + recoverDefencePower * Time.fixedDeltaTime);
        MovePower = Mathf.Clamp01(MovePower + recoverMovePower * Time.fixedDeltaTime);

        // Move
        thisBody.velocity = Vector3.zero;
        thisBody.angularVelocity = Vector3.zero;
        if (enablePathfinder)
        {
            if (thisBody.position != destination)
            {
                if (TestObstacle(thisBody.position, destination) == 0)
                {
                    moveBeacons.Clear();
                    moveBeacons.Add(destination);
                }
                if (moveBeacons.Count != 0)
                {
                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                    Vector3 rotateDirection = moveVector.normalized;
                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * Mathf.Clamp01((thisBody.position - destination).magnitude / slowDownRadius);
                    if (moveVector.magnitude <= moveDistance)
                    {
                        if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                        {
                            FindPath(thisBody.position, destination);
                            return;
                        }
                        thisBody.position = moveBeacons[0];
                        moveBeacons.RemoveAt(0);
                    }
                    else
                    {
                        if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                        {
                            FindPath(thisBody.position, destination);
                        }
                        thisBody.position += transform.forward * moveDistance;
                    }
                }
                else
                {
                    FindPath(thisBody.position, destination);
                }
            }
        }
        else
        {
            if (forcedMoveDestinations.Count != 0)
            {
                Vector3 moveVector = forcedMoveDestinations[0] - thisBody.position;
                Vector3 rotateDirection = moveVector.normalized;
                thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * MovePower;

                if (moveVector.magnitude <= moveDistance)
                {
                    thisBody.position = forcedMoveDestinations[0];
                    forcedMoveDestinations.RemoveAt(0);
                }
                else
                {
                    thisBody.position += moveVector.normalized * moveDistance;
                }
            }
            else
            {
                SetDestination(transform.position);
            }
        }
    }

    protected override void OnCreatedAction()
    {
        base.OnCreatedAction();
        AttackPower = 1;
        DefencePower = 1;
        MovePower = 1;
    }

    protected override void OnDestroyedAction()
    {
        base.OnDestroyedAction();
    }

    // Move
    private float TestObstacle(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        List<Collider> toIgnore = new List<Collider>(Physics.OverlapSphere(from, agentRadius));
        RaycastHit[] hits = Physics.CapsuleCastAll(from, from + direction * agentRadius * 5, agentRadius, direction, direction.magnitude, ~pathfinderLayerMask);
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
        List<Collider> intersectObjects = new List<Collider>(Physics.OverlapSphere(to, agentRadius, ~pathfinderLayerMask));
        intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
        float nextStepDistance = searchStepDistance;
        bool find = false;
        if (intersectObjects.Count != 0)
        {
            while (nextStepDistance <= searchStepMaxDistance && !find)
            {
                for (int i = 0; i < searchMaxRandomNumber; i++)
                {
                    Vector3 newDestination = to + nextStepDistance * new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1).normalized;
                    intersectObjects = new List<Collider>(Physics.OverlapSphere(newDestination, agentRadius, ~pathfinderLayerMask));
                    intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                    if (intersectObjects.Count == 0)
                    {
                        destination = to = newDestination;
                        find = true;
                        break;
                    }
                }
                nextStepDistance += searchStepDistance;
            }
        }
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
                    intersectObjects = new List<Collider>(Physics.OverlapSphere(middle, agentRadius, ~pathfinderLayerMask));
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
                result.Clear();
            }
            else
            {
                result.Insert(0, middle);
            }
        }
        moveBeacons = result;
    }
}
