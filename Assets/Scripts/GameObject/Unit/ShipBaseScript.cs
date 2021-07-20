using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability;

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
            // 0.5 can 
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
            }
            AttackPower = Mathf.Clamp01(AttackPower + recoverAttackPower * Time.fixedDeltaTime);
            DefencePower = Mathf.Clamp01(DefencePower + recoverDefencePower * Time.fixedDeltaTime);
            MovePower = Mathf.Clamp01(MovePower + recoverMovePower * Time.fixedDeltaTime);

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
                            if (moveBeacons.Count != 0)
                            {
                                Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                Vector3 rotateDirection = moveVector.normalized;
                                rotateDirection.y = 0;
                                thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
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
                        thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation,
                            Quaternion.LookRotation((finalRotationTarget - thisBody.position).normalized),
                            Time.fixedDeltaTime * agentRotateSpeed);
                        if (Vector3.Angle(transform.forward, (finalRotationTarget - thisBody.position).normalized) <= 0.1f)
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
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    rotateDirection.y = 0;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                    moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                    lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
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
                                if (moveBeacons.Count != 0)
                                {
                                    Vector3 moveVector = moveBeacons[0] - thisBody.position;
                                    Vector3 rotateDirection = moveVector.normalized;
                                    rotateDirection.y = 0;
                                    thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * agentRotateSpeed);
                                    float moveSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(rotateDirection)));
                                    moveSpeedAdjust = (moveSpeedAdjust + 1) / 2;
                                    lastFrameSpeedAdjust = Mathf.Cos(Mathf.Deg2Rad * Quaternion.Angle(thisBody.rotation, Quaternion.LookRotation(lastFrameMoveDirection)));
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
            foreach (AnchorData i in subsyetemAnchors)
            {
                if (i.subsystem != null)
                {
                    GameManager.GameManagerInstance.OnGameObjectDestroyed(i.subsystem, lastDamagedBy);
                }
            }
            base.OnDestroyedAction();
        }

        // Pathfinder
        private float TestObstacle(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = (to - from).magnitude;
            List<Collider> toIgnore = new List<Collider>() { GetComponent<Collider>() };
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
            List<Collider> toIgnore = new List<Collider>() { GetComponent<Collider>() };
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