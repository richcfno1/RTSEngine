using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability.SpecialAbility;

namespace RTS.RTSGameObject.Unit
{
    public class ShipBaseScript : UnitBaseScript
    {
        [Header("Move")]
        [Tooltip("Move speed.")]
        public float agentMoveSpeed;
        [Tooltip("Rotate speed.")]
        public float agentRotateSpeed;
        [Tooltip("The limitation of max velocity change.")]
        public float agentAccelerateLimit;

        private List<Vector3> agentCorners = new List<Vector3>();
        private float lastFrameSpeedAdjust = 0;
        private Vector3 lastFrameMoveDirection = new Vector3();
        private Rigidbody thisBody;

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
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
            finalPosition = transform.position;
            thisBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (HP <= 0)
            {
                OnDestroyedAction();
                return;
            }
            curAttackPower = Mathf.Clamp(curAttackPower + recoverAttackPower * Time.fixedDeltaTime, 0, maxAttackPower);
            curDefencePower = Mathf.Clamp(curDefencePower + recoverDefencePower * Time.fixedDeltaTime, 0, maxDefencePower);
            curMovePower = Mathf.Clamp(curMovePower + recoverMovePower * Time.fixedDeltaTime, 0, maxMovePower);

            // Action
            // Do not play physcial simulation here, this is a spaceship!
            thisBody.velocity = Vector3.zero;
            thisBody.angularVelocity = Vector3.zero;
            searchTimer += Time.fixedDeltaTime;

            // In aggressive status, when detect a nearby enemy, call attack ability
            if (AttackAbility != null && CurrentFireControlStatus == FireControlStatus.Aggressive && autoEngageTarget == null && 
                (ActionQueue.Count == 0 || ActionQueue.First().actionType != ActionType.ForcedMove))
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
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.Move:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
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
                            if (moveBeacons.Count != 0)
                            {
                                Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                Vector3 rotateDirection = moveVector.normalized;
                                rotateDirection.y = 0;
                                thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                                lastFrameSpeedAdjust = (lastFrameSpeedAdjust + 1) / 2;
                                moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, 0, lastFrameSpeedAdjust + agentAccelerateLimit);
                                float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * moveSpeedAdjust * MovePower;

                                lastFrameSpeedAdjust = moveSpeedAdjust;
                                lastFrameMoveDirection = (moveVector * moveDistance).normalized;
                                if (moveVector.magnitude <= moveDistance)
                                {
                                    if (!TestObstacleAndPush(thisBody.position, moveBeacons[0]))
                                    {
                                        ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                        return;
                                    }
                                    thisBody.position = moveBeacons[0];
                                    moveBeacons.RemoveAt(0);
                                }
                                else
                                {
                                    if (!TestObstacleAndPush(thisBody.position, thisBody.position + moveVector.normalized * moveDistance))
                                    {
                                        ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                        return;
                                    }
                                    thisBody.position += moveVector.normalized * moveDistance;
                                }
                            }
                            else
                            {
                                ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                            }
                        }
                        else
                        {
                            lastFrameSpeedAdjust = 0;
                            ActionQueue.RemoveFirst();
                        }
                        return;
                    case ActionType.LookAt:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        finalRotationTarget = (Vector3)action.targets[0];
                        thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation,
                            Quaternion.LookRotation((finalRotationTarget - thisBody.position).normalized),
                            Time.fixedDeltaTime * agentRotateSpeed);
                        if (Vector3.Angle(transform.forward, (finalRotationTarget - thisBody.position).normalized) <= 0.1f)
                        {
                            ActionQueue.RemoveFirst();
                        }
                        return;
                    case ActionType.LookAtTarget:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
                        {
                            return;
                        }
                        // Action implementation
                        if ((GameObject)action.targets[0] == null)
                        {
                            ActionQueue.RemoveFirst();
                        }
                        else
                        {
                            finalRotationTarget = ((GameObject)action.targets[0]).transform.position;
                            thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation,
                                Quaternion.LookRotation((finalRotationTarget - thisBody.position).normalized),
                                Time.fixedDeltaTime * agentRotateSpeed);
                        }
                        return;
                    case ActionType.Follow:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
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
                            if ((thisBody.position - followTarget.transform.position).magnitude < ((Vector3)action.targets[1]).magnitude)
                            {
                                finalPosition = thisBody.position;
                            }
                            else
                            {
                                finalPosition = followTarget.transform.position + (Vector3)action.targets[1];
                            }
                            finalPosition = followTarget.transform.position + (Vector3)action.targets[1];
                            if (thisBody.position != finalPosition)
                            {
                                // Moving
                                if (TestObstacle(thisBody.position, finalPosition) == 0)
                                {
                                    moveBeacons.Clear();
                                    moveBeacons.Add(finalPosition);
                                }
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    rotateDirection.y = 0;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                    moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                    lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                                    lastFrameSpeedAdjust = (lastFrameSpeedAdjust + 1) / 2;
                                    moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, 0, lastFrameSpeedAdjust + agentAccelerateLimit);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * moveSpeedAdjust * MovePower;

                                    lastFrameSpeedAdjust = moveSpeedAdjust;
                                    lastFrameMoveDirection = (moveVector * moveDistance).normalized;
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, moveBeacons[0]))
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, thisBody.position + moveVector.normalized * moveDistance))
                                        {
                                            ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                            return;
                                        }
                                        thisBody.position += moveVector.normalized * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - followTarget.transform.position;
                                }
                            }
                            else
                            {
                                lastFrameSpeedAdjust = 0;
                            }
                        }
                        return;
                    case ActionType.KeepInRange:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
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
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    rotateDirection.y = 0;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                    moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                    lastFrameSpeedAdjust =  Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                                    lastFrameSpeedAdjust = (lastFrameSpeedAdjust + 1) / 2;
                                    moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, 0, lastFrameSpeedAdjust + agentAccelerateLimit);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * moveSpeedAdjust * MovePower;

                                    lastFrameSpeedAdjust = moveSpeedAdjust;
                                    lastFrameMoveDirection = (moveVector * moveDistance).normalized;
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, moveBeacons[0]))
                                        {
                                            FindPath(thisBody.position, finalPosition);
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, thisBody.position + moveVector.normalized * moveDistance))
                                        {
                                            FindPath(thisBody.position, finalPosition);
                                            return;
                                        }
                                        thisBody.position += moveVector.normalized * moveDistance;
                                    }
                                }
                                else
                                {
                                    FindPath(thisBody.position, finalPosition);
                                }
                            }
                            else
                            {
                                lastFrameSpeedAdjust = 0;
                            }
                        }
                        return;
                    case ActionType.KeepInRangeAndLookAt:
                        // Ability check
                        if (MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        else if (!MoveAbility.CanUseAbility())
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
                            // It is important to use +1 and -1 to avoid unit freezing at a point
                            isApproaching = isApproaching ?
                                currentDistance > (float)action.targets[3] + 1 :  // If is approaching and not close enough
                                currentDistance >= (float)action.targets[2] - 1;  // If is not approaching but be too far away

                            if (isApproaching)
                            {
                                finalPosition = keepInRangeAndHeadToTarget.transform.position + 
                                    ((Vector3)action.targets[1]).normalized * ((float)action.targets[3]);

                                if (thisBody.position != finalPosition)
                                {
                                    // Moving
                                    if (TestObstacle(thisBody.position, finalPosition) == 0)
                                    {
                                        moveBeacons.Clear();
                                        moveBeacons.Add(finalPosition);
                                    }
                                    if (moveBeacons.Count != 0)
                                    {
                                        Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                        Vector3 rotateDirection = moveVector.normalized;
                                        rotateDirection.y = 0;
                                        thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                        float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                        moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                        lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                                        lastFrameSpeedAdjust = (lastFrameSpeedAdjust + 1) / 2;
                                        moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, 0, lastFrameSpeedAdjust + agentAccelerateLimit);
                                        float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * moveSpeedAdjust * MovePower;

                                        lastFrameSpeedAdjust = moveSpeedAdjust;
                                        lastFrameMoveDirection = (moveVector * moveDistance).normalized;
                                        if (moveVector.magnitude <= moveDistance)
                                        {
                                            if (!TestObstacleAndPush(thisBody.position, moveBeacons[0]))
                                            {
                                                ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                                return;
                                            }
                                            thisBody.position = moveBeacons[0];
                                            moveBeacons.RemoveAt(0);
                                        }
                                        else
                                        {
                                            if (!TestObstacleAndPush(thisBody.position, thisBody.position + moveVector.normalized * moveDistance))
                                            {
                                                ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                                return;
                                            }
                                            thisBody.position += moveVector.normalized * moveDistance;
                                        }
                                    }
                                    else
                                    {
                                        ActionQueue.First().targets[1] = FindPath(thisBody.position, finalPosition) - keepInRangeAndHeadToTarget.transform.position;
                                    }
                                }
                                else
                                {
                                    lastFrameSpeedAdjust = 0;
                                }
                            }
                            else
                            {
                                finalRotationTarget = keepInRangeAndHeadToTarget.transform.position;
                                thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, 
                                    Quaternion.LookRotation((finalRotationTarget - thisBody.position).normalized), 
                                    Time.fixedDeltaTime * agentRotateSpeed);
                            }
                        }
                        return;
                    case ActionType.Attack:
                        // Ability check
                        if (AttackAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            Follow((GameObject)action.targets[0]);
                            return;
                        }
                        else if (AttackAbility.CanUseAbility())
                        {
                            ActionQueue.RemoveFirst();
                            AttackAbility.HandleAttackAction((GameObject)action.targets[0]);
                            return;
                        }
                        Follow((GameObject)action.targets[0]);
                        return;
                    case ActionType.AttackAndMove:
                        // Ability check
                        if (AttackAbility == null && MoveAbility == null)
                        {
                            ActionQueue.RemoveFirst();
                            return;
                        }
                        if (MoveAbility != null && MoveAbility.CanUseAbility())
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
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    rotateDirection.y = 0;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                    moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                    lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
                                    lastFrameSpeedAdjust = (lastFrameSpeedAdjust + 1) / 2;
                                    moveSpeedAdjust = Mathf.Clamp(moveSpeedAdjust, 0, lastFrameSpeedAdjust + agentAccelerateLimit);
                                    float moveDistance = agentMoveSpeed * Time.fixedDeltaTime * moveSpeedAdjust * MovePower;

                                    lastFrameSpeedAdjust = moveSpeedAdjust;
                                    lastFrameMoveDirection = (moveVector * moveDistance).normalized;
                                    if (moveVector.magnitude <= moveDistance)
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, moveBeacons[0]))
                                        {
                                            ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                            return;
                                        }
                                        thisBody.position = moveBeacons[0];
                                        moveBeacons.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (!TestObstacleAndPush(thisBody.position, thisBody.position + moveVector.normalized * moveDistance))
                                        {
                                            ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                            return;
                                        }
                                        thisBody.position += moveVector.normalized * moveDistance;
                                    }
                                }
                                else
                                {
                                    ActionQueue.First().targets[0] = FindPath(thisBody.position, finalPosition);
                                }
                            }
                            else
                            {
                                lastFrameSpeedAdjust = 0;
                                ActionQueue.RemoveFirst();
                            }
                        }
                        if (AttackAbility != null && AttackAbility.CanUseAbility())
                        {
                            if (AttackAbility != null && autoEngageTarget == null)
                            {
                                Collider temp = Physics.OverlapSphere(transform.position, autoEngageDistance).
                                    FirstOrDefault(x => x.GetComponent<UnitBaseScript>() != null && x.GetComponent<UnitBaseScript>().BelongTo != BelongTo);
                                if (temp != null)
                                {
                                    Attack(temp.gameObject);
                                    autoEngageTarget = temp.gameObject;
                                }
                            }
                        }
                        return;
                    case ActionType.UseNoSelectionSpecialAbility:
                        ((NoSelectionSpecialAbilityScript)action.targets[0]).ParseSpecialAbility();
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.UseSelectTargetSpecialAbility:
                        ((SelectTargetSpecialAbilityScript)action.targets[0]).ParseSpecialAbility((GameObject)action.targets[1]);
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.UseSelectSpaceSpecialAbility:
                        ((SelectSpaceSpecialAbilityScript)action.targets[0]).ParseSpecialAbility((Vector3)action.targets[1]);
                        ActionQueue.RemoveFirst();
                        return;
                    case ActionType.ForcedMove:
                        finalPosition = (Vector3)action.targets[0];
                        if (thisBody.position != finalPosition)
                        {
                            // Disable collider
                            List<Collider> allColliders = new List<Collider>();
                            allColliders.AddRange(GetComponentsInChildren<Collider>());
                            allColliders.RemoveAll(x => x.gameObject.layer == 11);
                            foreach (Collider i in allColliders)
                            {
                                i.enabled = false;
                            }

                            // Move
                            Vector3 moveVector = finalPosition - thisBody.position;
                            Vector3 rotateDirection = moveVector.normalized;
                            rotateDirection.y = 0;  // Consider to allow rotation in y?
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

        // Pathfinder
        private float TestObstacle(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = (to - from).magnitude;
            List<Collider> toIgnore = new List<Collider>();
            toIgnore.AddRange(GetComponentsInChildren<Collider>());
            foreach (Vector3 i in agentCorners)
            {
                RaycastHit hit;
                if (Physics.Raycast(from - transform.position + transform.TransformPoint(i), direction, out hit, distance))
                {
                    if (!toIgnore.Contains(hit.collider) && hit.collider.GetComponentInParent<RTSGameObjectBaseScript>() != null)
                    {
                        if (objectScale <= hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().objectScale)
                        {
                            return (hit.collider.ClosestPoint(from) - from).magnitude;
                        }
                    }
                }
            }
            return 0;
        }

        private bool TestObstacleAndPush(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = (to - from).magnitude;
            List<Collider> toIgnore = new List<Collider>();
            toIgnore.AddRange(GetComponentsInChildren<Collider>());
            List<RTSGameObjectBaseScript> avoidInfo = new List<RTSGameObjectBaseScript>();
            foreach (Vector3 i in agentCorners)
            {
                RaycastHit hit;
                if (Physics.Raycast(from - transform.position + transform.TransformPoint(i), direction, out hit, distance * NavigationCollider.size.magnitude))
                {
                    if (!toIgnore.Contains(hit.collider))
                    {
                        if (hit.collider.GetComponentInParent<RTSGameObjectBaseScript>() == null)
                        {
                            continue;
                        }
                        if (objectScale <= hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().objectScale)
                        {
                            return false;
                        }
                        else
                        {
                            avoidInfo.Add(hit.collider.GetComponentInParent<RTSGameObjectBaseScript>());
                        }
                    }
                }
            }
            return true;
        }

        // Return value is modified destination
        private Vector3 FindPath(Vector3 from, Vector3 to)
        {
            if (searchTimer < nextSearchPending)
            {
                Debug.Log("Search pending");
                return finalPosition;
            }
            List<Vector3> result = new List<Vector3>();
            List<Collider> intersectObjects = new List<Collider>(Physics.OverlapBox(to, NavigationCollider.size, transform.rotation));
            float nextStepDistance = searchStepDistance;
            bool find = intersectObjects.Count == 0;
            if (intersectObjects.Count != 0)
            {
                while (nextStepDistance <= searchStepMaxDistance && !find)
                {
                    for (int i = 0; i < searchMaxRandomNumber; i++)
                    {
                        Vector3 newDestination = to + nextStepDistance *
                            new Vector3(UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1).normalized;
                        intersectObjects = new List<Collider>(Physics.OverlapBox(newDestination, NavigationCollider.size, transform.rotation));
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
            if (!find)
            {
                Debug.Log("Out of search limitation when determine alternative destination");
            }
            result.Add(to);
            float obstacleDistance = TestObstacle(from, to);
            if (obstacleDistance != 0)
            {
                Vector3 direction = (to - from).normalized;
                Vector3 obstaclePosition = from + obstacleDistance * direction;
                Vector3 middle = new Vector3();
                Plane tempPlane = new Plane(direction, obstaclePosition);
                Vector3 searchDirectionInPlane1 = tempPlane.ClosestPointOnPlane(to + new Vector3(0, 1, 0)) - obstaclePosition;
                searchDirectionInPlane1 = searchDirectionInPlane1.normalized;
                Vector3 searchDirectionInPlane2 = Vector3.Cross(direction, searchDirectionInPlane1).normalized;
                nextStepDistance = searchStepDistance;
                find = false;
                while (nextStepDistance <= searchStepMaxDistance && !find)
                {
                    for (int i = 0; i < searchMaxRandomNumber; i++)
                    {
                        middle = obstaclePosition + nextStepDistance * (searchDirectionInPlane1 * UnityEngine.Random.value +
                            searchDirectionInPlane2 * UnityEngine.Random.value).normalized;
                        intersectObjects = new List<Collider>(Physics.OverlapBox(middle, NavigationCollider.size, transform.rotation));
                        intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                        if (intersectObjects.Count == 0 && TestObstacle(middle, to) == 0 && TestObstacle(from, middle) == 0)
                        {
                            find = true;
                            break;
                        }
                    }
                    nextStepDistance += searchStepDistance;
                }
                if (!find)
                {
                    Debug.Log("Out of search limitation when determine path");
                    searchTimer = 0;
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