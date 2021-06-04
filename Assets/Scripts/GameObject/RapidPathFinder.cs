using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapidPathFinder : MonoBehaviour
{
    // Move
    public float agentMoveSpeed;
    public float agentRotateSpeed;

    // Search
    public float agentRadius;
    public float searchStepDistance;
    public float searchStepMaxDistance;
    public int searchMaxRandomNumber;

    private Vector3 destination;
    private List<Vector3> moveBeacons = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
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

                Vector3 moveDirection = moveBeacons[0] - transform.position;
                Vector3 rotateDirection = moveDirection.normalized;
                rotateDirection.y = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rotateDirection), Time.deltaTime * agentRotateSpeed);

                float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(transform.rotation, Quaternion.LookRotation(rotateDirection)));
                moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                float moveDistance = agentMoveSpeed * Time.deltaTime * moveSpeedAdjust;
                if (moveDirection.magnitude <= moveDistance)
                {
                    transform.position = moveBeacons[0];
                    moveBeacons.RemoveAt(0);
                }
                else
                {
                    transform.position += moveDirection.normalized * moveDistance;
                }
            }
            else
            {
                moveBeacons = FindPath(transform.position, destination);
            }
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

    public void Move(Vector3 newDestination)
    {
        destination = newDestination;
        moveBeacons = FindPath(transform.position, destination);
    }

    public void Stop()
    {
        destination = transform.position;
    }
}
