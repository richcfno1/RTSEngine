using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAbilityScript : ContinuousAbilityBaseScript
{
    // Move
    private float agentMoveSpeed;
    private float agentRotateSpeed;
    private float agentAccelerateLimit;

    // Search
    private float agentRadius;
    private float searchStepDistance;
    private float searchStepMaxDistance;
    private float searchMaxRandomNumber;

    private Vector3 destination;
    private List<Vector3> moveBeacons = new List<Vector3>();
    private float lastFrameSpeedAdjust = 0;
    private Vector3 lastFrameMoveDirection = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Set from parent's property dict
        agentMoveSpeed = Parent.PropertyDictionary["MoveSpeed"];
        agentRotateSpeed = Parent.PropertyDictionary["RotateSpeed"];
        agentAccelerateLimit = Parent.PropertyDictionary["AccelerateLimit"];
        
        agentRadius = Parent.PropertyDictionary["MoveAgentRadius"];
        searchStepDistance = Parent.PropertyDictionary["MoveSearchStepDistance"];
        searchStepMaxDistance = Parent.PropertyDictionary["MoveSearchStepLimit"];
        searchMaxRandomNumber = Parent.PropertyDictionary["MoveSearchRandomNumber"];

        if (isUsing)
        {
            ContinuousAction(abilityTarget);
        }
    }

    private float TestObstacle(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float length = (to - from).magnitude;
        int hitCount = 0;
        float hitDistance = 0;
        for (int i = 0; i < searchMaxRandomNumber; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(from + agentRadius * new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1).normalized, direction, out hit, length))
            {
                if (hit.transform.gameObject != gameObject)
                {
                    hitDistance += hit.distance;
                    hitCount++;
                }
            }
        }
        if (hitDistance == 0)
        {
            return 0;
        }
        else
        {
            return hitDistance / hitCount;
        }
    }

    private List<Vector3> FindPath(Vector3 from, Vector3 to)
    {
        List<Vector3> result = new List<Vector3>();
        Collider[] intersectObjects = Physics.OverlapSphere(to, agentRadius);
        float nextStepDistance = searchStepDistance;
        bool find = false;
        if (intersectObjects.Length != 0)
        {
            while (nextStepDistance <= searchStepMaxDistance && !find)
            {
                for (int i = 0; i < searchMaxRandomNumber; i++)
                {
                    Vector3 newDestination = to + nextStepDistance * new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1).normalized;
                    intersectObjects = Physics.OverlapSphere(newDestination, agentRadius);
                    if (intersectObjects.Length == 0)
                    {
                        to = newDestination;
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
                    intersectObjects = Physics.OverlapSphere(middle, agentRadius);
                    if (intersectObjects.Length == 0 && TestObstacle(middle, to) == 0 && TestObstacle(from, middle) == 0)
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
                return new List<Vector3>();
            }
            else
            {
                result.Insert(0, middle);
            }
        }
        return result;
    }

    public override bool UseAbility(object target)
    {
        if (target.GetType() != typeof(Vector3))
        {
            abilityTarget = null;
            return isUsing = false;
        }
        abilityTarget = target;
        return base.UseAbility(target);
    }

    public override void PauseAbility()
    {
        base.PauseAbility();
    }

    public override void ContinuousAction(object target)
    {
        if (target.GetType() != typeof(Vector3))
        {
            return;
        }
        destination = (Vector3)target;
        if (transform.position != destination)
        {
            if (TestObstacle(transform.position, destination) == 0)
            {
                moveBeacons.Clear();
                moveBeacons.Add(destination);
            }
            if (moveBeacons.Count != 0)
            {
                if (TestObstacle(transform.position, moveBeacons[0]) != 0)
                {
                    moveBeacons = FindPath(transform.position, destination);
                    return;
                }

                Vector3 moveVector = moveBeacons[0] - transform.position;
                Vector3 rotateDirection = moveVector.normalized;
                rotateDirection.y = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rotateDirection), Time.deltaTime * agentRotateSpeed);

                float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(transform.rotation, Quaternion.LookRotation(rotateDirection)));
                moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(transform.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, lastFrameSpeedAdjust - agentAccelerateLimit, lastFrameSpeedAdjust + agentAccelerateLimit);
                float moveDistance = agentMoveSpeed * Time.deltaTime * moveSpeedAdjust;

                lastFrameSpeedAdjust = moveSpeedAdjust;
                lastFrameMoveDirection = moveVector.normalized;

                if (moveVector.magnitude <= moveDistance)
                {
                    transform.position = moveBeacons[0];
                    moveBeacons.RemoveAt(0);
                }
                else
                {
                    transform.position += moveVector.normalized * moveDistance;
                }
            }
            else
            {
                moveBeacons = FindPath(transform.position, destination);
            }
        }
    }
}
