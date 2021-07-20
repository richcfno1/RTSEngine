using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability;

namespace RTS.RTSGameObject.Unit
{
    public class FighterBaseScript : UnitBaseScript
    {
        [Header("Move")]
        [Tooltip("Move speed.")]
        public float agentMoveSpeed;
        [Tooltip("Rotate speed.")]
        public float agentRotateSpeed;

        private float agentRadius;
        private float slowDownRadius;
        private Rigidbody thisBody;

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
            agentRadius = NavigationCollider.size.magnitude;
            finalPosition = transform.position;
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

            // Action
            // Do not play physcial simulation here, this is a spaceship!
            thisBody.velocity = Vector3.zero;
            thisBody.angularVelocity = Vector3.zero;

            // In aggressive status, when detect a nearby enemy, call attack ability
            if (GetComponent<AttackAbilityScript>() != null && CurrentFireControlStatus == FireControlStatus.Aggressive && autoEngageTarget == null)
            {
                Collider temp = Physics.OverlapSphere(transform.position, autoEngageDistance).
                    FirstOrDefault(x => x.GetComponent<UnitBaseScript>() != null && x.GetComponent<UnitBaseScript>().BelongTo != BelongTo);
                if (temp != null)
                {
                    Attack(temp.gameObject);
                    autoEngageTarget = temp.gameObject;
                }
            }

            if (ActionQueue.Count != 0)
            {
                UnitAction action = ActionQueue.First();
                switch (action.actionType)
                {
                    case ActionType.Stop:
                        if (ActionQueue.Count == 1)
                        {
                            CurrentFireControlStatus = fireControlStatusBeforeOverride;
                        }
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.Move:
                        // Ability check
                        if (GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        finalPosition = (Vector3)action.targets[0];
                        if (thisBody.position != finalPosition)
                        {
                            // Moving
                            if (TestObstacle(thisBody.position, finalPosition) == 0)
                            {
                                moveBeacons.Clear();
                                moveBeacons.Add(finalPosition);
                            }
                            if (TestObstacle(thisBody.position, finalPosition) == 0)
                            {
                                moveBeacons.Clear();
                                moveBeacons.Add(finalPosition);
                            }
                            if (moveBeacons.Count != 0)
                            {
                                Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                Vector3 rotateDirection = moveVector.normalized;
                                thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * Mathf.Clamp01((thisBody.position - finalPosition).magnitude / slowDownRadius);
                                if (moveVector.magnitude <= moveDistance)
                                {
                                    if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                                    {
                                        ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                        return;
                                    }
                                    thisBody.position = moveBeacons[0];
                                    moveBeacons.RemoveAt(0);
                                }
                                else
                                {
                                    if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                                    {
                                        ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                    }
                                    thisBody.position += transform.forward * moveDistance;
                                }
                            }
                            else
                            {
                                ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                            }
                        }
                        else
                        {
                            ActionQueue.RemoveFirst();
                        }
                        return;
                    case ActionType.HeadTo:
                        // Ability check
                        if (GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        finalRotationTarget = (Vector3)action.targets[0];
                        Vector3 rotateTo = (finalRotationTarget - thisBody.position).normalized;
                        rotateTo.y = 0;  // Consider to allow rotation in y?
                        thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateTo), Time.fixedDeltaTime * agentRotateSpeed);
                        if (Vector3.Angle(transform.forward, rotateTo) <= 0.1f)
                        {
                            ActionQueue.RemoveFirst();
                        }
                        return;
                    case ActionType.Follow:
                        // Ability check
                        if (GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        GameObject followTarget = (GameObject)action.targets[0];
                        if (followTarget == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else
                        {
                            finalPosition = followTarget.transform.position + (Vector3)action.targets[1];
                            if (thisBody.position != finalPosition)
                            {
                                // Moving
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * Mathf.Clamp01((thisBody.position - finalPosition).magnitude / slowDownRadius);
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                        }
                                        thisBody.position += transform.forward * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                }
                            }
                        }
                        return;
                    case ActionType.KeepInRange:
                        // Ability check
                        if (GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        GameObject keepInRangeTarget = (GameObject)action.targets[0];
                        if (keepInRangeTarget == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else
                        {
                            if ((thisBody.position - keepInRangeTarget.transform.position).magnitude < (float)action.targets[1] &&
                                (thisBody.position - keepInRangeTarget.transform.position).magnitude > (float)action.targets[2])
                            {
                                finalPosition = thisBody.position;
                            }
                            else
                            {
                                Vector3 currentDirection = (transform.position - keepInRangeTarget.transform.position).normalized;
                                finalPosition = keepInRangeTarget.transform.position + currentDirection * 
                                    ((float)action.targets[1] + (float)action.targets[2]) / 2;
                            }
                            for (int i = 0; i < searchMaxRandomNumber; i++)
                            {
                                if (Physics.RaycastAll(finalPosition, (keepInRangeTarget.transform.position - finalPosition).normalized,
                                    (keepInRangeTarget.transform.position - finalPosition).magnitude).
                                    Where(x => !transform.GetComponentsInChildren<Collider>().Contains(x.collider) &&
                                    !keepInRangeTarget.transform.GetComponentsInChildren<Collider>().Contains(x.collider)).ToArray().Length == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    finalPosition = keepInRangeTarget.transform.position + 
                                        new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized *
                                        ((float)action.targets[1] + (float)action.targets[2]) / 2;
                                }
                            }
                            
                            if (thisBody.position != finalPosition)
                            {
                                // Moving
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * Mathf.Clamp01((thisBody.position - finalPosition).magnitude / slowDownRadius);
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeTarget.transform.position;
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeTarget.transform.position;
                                        }
                                        thisBody.position += transform.forward * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeTarget.transform.position;
                                }
                            }
                        }
                        return;
                    case ActionType.KeepInRangeAndHeadTo:
                        // Ability check
                        if (GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        GameObject keepInRangeAndHeadToTarget = (GameObject)action.targets[0];
                        if (keepInRangeAndHeadToTarget == null)
                        {
                            ActionQueue.RemoveFirst();
                            isApproaching = true;
                            return;
                        }
                        else
                        {
                            float currentDistance = (keepInRangeAndHeadToTarget.transform.position - transform.position).magnitude;
                            Vector3 currentDirection = (transform.position - keepInRangeAndHeadToTarget.transform.position).normalized;
                            // It is important to use +1 and -1 to avoid unit freezing at a point
                            isApproaching = isApproaching ?
                                currentDistance > (float)action.targets[3] + 1 :  // If is approaching and not close enough
                                currentDistance >= (float)action.targets[2] - 1;  // If is not approaching but be too far away

                            finalPosition = isApproaching ? 
                                keepInRangeAndHeadToTarget.transform.position + currentDirection * (float)action.targets[3] :
                                keepInRangeAndHeadToTarget.transform.position + currentDirection * (float)action.targets[2];

                            if (thisBody.position != finalPosition)
                            {
                                // Moving
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime;
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                        }
                                        thisBody.position += transform.forward * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                }
                            }
                        }
                        return;
                    case ActionType.Attack:
                        // Ability check
                        if (GetComponent<AttackAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (GetComponent<AttackAbilityScript>().CanUseAbility())
                        {
                            ActionQueue.RemoveFirst();
                            GetComponent<AttackAbilityScript>().HandleAttackAction((GameObject)action.targets[0]);
                            return;
                        }
                        Follow((GameObject)action.targets[0]);
                        return;
                    case ActionType.AttackAndMove:
                        // Ability check
                        if (GetComponent<AttackAbilityScript>() == null && GetComponent<MoveAbilityScript>() == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!GetComponent<AttackAbilityScript>().CanUseAbility() && !GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            return;
                        }
                        if (GetComponent<MoveAbilityScript>() != null && GetComponent<MoveAbilityScript>().CanUseAbility())
                        {
                            finalPosition = (Vector3)action.targets[0];
                            if (thisBody.position != finalPosition)
                            {
                                // Moving
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * Mathf.Clamp01((thisBody.position - finalPosition).magnitude / slowDownRadius);
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (TestObstacle(thisBody.position, moveBeacons[0]) != 0)
                                        {
                                            ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (TestObstacle(thisBody.position, thisBody.position + transform.forward * moveDistance) != 0)
                                        {
                                            ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                        }
                                        thisBody.position += transform.forward * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                }
                            }
                            else
                            {
                                ActionQueue.RemoveFirst();
                            }
                            return;
                        }
                        if (GetComponent<AttackAbilityScript>() != null && GetComponent<AttackAbilityScript>().CanUseAbility())
                        {
                            if (GetComponent<AttackAbilityScript>() != null && autoEngageTarget == null)
                            {
                                Collider temp = Physics.OverlapSphere(transform.position, autoEngageDistance).
                                    FirstOrDefault(x => x.GetComponent<UnitBaseScript>() != null && x.GetComponent<UnitBaseScript>().BelongTo != BelongTo);
                                if (temp != null)
                                {
                                    Attack(temp.gameObject);
                                    autoEngageTarget = temp.gameObject;
                                }
                            }
                            return;
                        }
                        return;
                    case ActionType.UseSpecialAbility:
                        Debug.LogWarning("Unimplemented: UseSpecialAbility");
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.ForcedMove:
                        finalPosition = (Vector3)action.targets[0];
                        if (thisBody.position != finalPosition)
                        {
                            // Disable collider
                            List<Collider> allColliders = new List<Collider>();
                            allColliders.AddRange(GetComponents<Collider>());
                            allColliders.AddRange(GetComponentsInChildren<Collider>());
                            allColliders.RemoveAll(x => x.gameObject.layer == 11);
                            foreach (Collider i in allColliders)
                            {
                                i.enabled = false;
                            }

                            // Move
                            Vector3 moveVector = finalPosition - thisBody.position;
                            Vector3 rotateDirection = moveVector.normalized;
                            thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                            float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * MovePower;

                            if (moveVector.magnitude <= moveDistance)
                            {
                                thisBody.position = finalPosition;
                            }
                            else
                            {
                                thisBody.position += moveVector.normalized * moveDistance;
                            }
                        }
                        else
                        {
                            // Enable collider
                            List<Collider> allColliders = new List<Collider>();
                            allColliders.AddRange(GetComponents<Collider>());
                            allColliders.AddRange(GetComponentsInChildren<Collider>());
                            allColliders.RemoveAll(x => x.gameObject.layer == 11);
                            foreach (Collider i in allColliders)
                            {
                                i.enabled = false;
                            }

                            ActionQueue.RemoveFirst();
                        }
                        return;
                    default:
                        Debug.LogError("Wrong type of action: " + action.actionType);
                        return;
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

        // Return value is modified destination
        private Vector3 FindPath(Vector3 from, Vector3 to)
        {
            List<Vector3> result = new List<Vector3>();
            List<Collider> intersectObjects = new List<Collider>(Physics.OverlapSphere(to, agentRadius));
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
                        intersectObjects = new List<Collider>(Physics.OverlapSphere(newDestination, agentRadius));
                        intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                        if (intersectObjects.Count == 0)
                        {
                            finalPosition = to = newDestination;
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
                    result.Clear();
                }
                else
                {
                    result.Insert(0, middle);
                }
            }
            moveBeacons = result;
            return finalPosition;
        }
    }
}