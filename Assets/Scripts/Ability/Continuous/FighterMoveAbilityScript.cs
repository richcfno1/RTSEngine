using System.Collections.Generic;
using UnityEngine;

public class FighterMoveAbilityScript : MoveAbilityScript
{
    // Start is called before the first frame update
    void Start()
    {
        // Set from parent's property dict
        agentRadius = Parent.NavigationCollider.GetComponent<SphereCollider>().radius;
        agentMoveSpeed = Parent.PropertyDictionary["MoveSpeed"];
        agentRotateSpeed = Parent.PropertyDictionary["RotateSpeed"];
        agentAccelerateLimit = Parent.PropertyDictionary["AccelerateLimit"];
        searchStepDistance = Parent.PropertyDictionary["MoveSearchStepDistance"];
        searchStepMaxDistance = Parent.PropertyDictionary["MoveSearchStepLimit"];
        searchMaxRandomNumber = Parent.PropertyDictionary["MoveSearchRandomNumber"];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isUsing)
        {
            ContinuousAction();
        }
    }

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
                moveBeacons.Clear();
            }
            else
            {
                result.Insert(0, middle);
            }
        }
        moveBeacons = result;
    }

    // For MoveAbility target size should be 2
    // target[0] = int where 0 = stop, 1 = moveTo
    // target[1] = Vector3, which is destination
    public override bool UseAbility(List<object> target)
    {
        if (target.Count != 2 || target[1].GetType() != typeof(Vector3))
        {
            abilityTarget = new List<object>();
            return isUsing = false;
        }
        moveBeacons.Clear();
        return base.UseAbility(target);
    }

    public override void PauseAbility()
    {
        base.PauseAbility();
    }

    protected override void ContinuousAction()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        // Decode and set action from abilityTarget
        if ((int)abilityTarget[0] == 0)
        {
            destination = transform.position;
        }
        else if ((int)abilityTarget[0] == 1)
        {
            destination = (Vector3)abilityTarget[1];
        }
        if (transform.position != destination)
        {
            if (TestObstacle(transform.position, destination) == 0)
            {
                moveBeacons.Clear();
                moveBeacons.Add(destination);
            }
            if (moveBeacons.Count != 0)
            {
                Vector3 moveVector = moveBeacons[0] - transform.position;
                Vector3 rotateDirection = moveVector.normalized;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                float moveDistance = agentMoveSpeed * Time.fixedDeltaTime;
                if (moveVector.magnitude <= moveDistance)
                {
                    if (TestObstacle(transform.position, moveBeacons[0]) != 0)
                    {
                        FindPath(transform.position, destination);
                        return;
                    }
                    transform.position = moveBeacons[0];
                    moveBeacons.RemoveAt(0);
                }
                else
                {
                    if (TestObstacle(transform.position, transform.position + transform.forward * moveDistance) != 0)
                    {
                        FindPath(transform.position, destination);
                        return;
                    }
                    transform.position += transform.forward * moveDistance;
                }
            }
            else
            {
                FindPath(transform.position, destination);
            }
        }
    }
}
