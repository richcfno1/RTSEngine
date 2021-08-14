using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability.SpecialAbility;
using RTS.Helper;
using RTS.RTSGameObject.Subsystem;

namespace RTS.RTSGameObject.Unit
{
    public class FighterBaseScript : UnitBaseScript
    {
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
            allColliders = GetComponentsInChildren<Collider>().ToList();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            SetSeed();
            if (HP <= 0)
            {
                OnDestroyedAction();
                return;
            }

            // Recover
            AttackPower = Mathf.Clamp01(AttackPower + recoverAttackPower * Time.fixedDeltaTime);
            DefencePower = Mathf.Clamp01(DefencePower + recoverDefencePower * Time.fixedDeltaTime);
            MovePower = Mathf.Clamp01(MovePower + recoverMovePower * Time.fixedDeltaTime);

            // Vision
            if (visionArea != null && BelongTo == GameManager.GameManagerInstance.selfIndex)
            {
                visionArea.transform.localScale = new Vector3(visionRange, visionRange, visionRange) * 2;
            }

            if (lockRotationZ)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            }

            // Action
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
                        if (AttackAbility != null)
                        {
                            AttackAbility.HandleAttackStop();
                        }
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
                        finalPosition = TestObstacleAround((Vector3)action.targets[0]);
                        if (MoveToFinalPosition())
                        {
                            ActionQueue.RemoveFirst();
                            moveBeacons.Clear();
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
                        ApplyRotation(finalRotationTarget);
                        if (Vector3.Angle(transform.forward, (finalRotationTarget - thisBody.position).normalized) <= maxErrorAngle)
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
                            ApplyRotation(finalRotationTarget);
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
                            finalPosition = TestObstacleAround(followTarget.transform.position + (Vector3)action.targets[1]);
                            action.targets[1] = finalPosition - followTarget.transform.position;
                            MoveToFinalPosition();
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
                                    Where(x =>
                                    {
                                        if (keepInRangeTarget.GetComponent<SubsystemBaseScript>() != null)
                                        {
                                            return !transform.GetComponentsInChildren<Collider>().Contains(x.collider) &&
                                            !keepInRangeTarget.GetComponent<SubsystemBaseScript>().Host.transform.GetComponentsInChildren<Collider>().Contains(x.collider);
                                        }
                                        else
                                        {
                                            return !transform.GetComponentsInChildren<Collider>().Contains(x.collider) &&
                                            !keepInRangeTarget.transform.GetComponentsInChildren<Collider>().Contains(x.collider);
                                        }
                                    }).ToList().Count == 0)
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
                            MoveToFinalPosition();
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
                            Vector3 currentDirection = (transform.position - keepInRangeAndHeadToTarget.transform.position).normalized;
                            // It is important to use +1 and -1 to avoid unit freezing at a point
                            isApproaching = isApproaching ?
                                currentDistance > (float)action.targets[3] + 1 :  // If is approaching and not close enough
                                currentDistance >= (float)action.targets[2] - 1;  // If is not approaching but be too far away

                            finalPosition = isApproaching ?
                                keepInRangeAndHeadToTarget.transform.position + currentDirection * (float)action.targets[3] :
                                keepInRangeAndHeadToTarget.transform.position + currentDirection * (float)action.targets[2];

                            MoveToFinalPosition(false);
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
                            finalPosition = TestObstacleAround((Vector3)action.targets[0]);
                            if (MoveToFinalPosition())
                            {
                                ActionQueue.RemoveFirst();
                                moveBeacons.Clear();
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
                        float speed = (float)action.targets[1];
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
                            thisBody.rotation = Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Mathf.Infinity);
                            float moveDistance = speed * Time.fixedDeltaTime * MovePower;

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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * maxRotationSpeed);
        }

        // Variable finalPosition must be valid
        // Return true if arrive beacon
        private bool MoveToFinalPosition(bool allowSlowDown = true)
        {
            if (Vector3.Distance(thisBody.position, finalPosition) > slowDownDistance)
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
                    ApplyForce(moveBeacons[0], moveBeacons.Count == 1);
                }
            }
            else if (allowSlowDown && Vector3.Distance(thisBody.position, finalPosition) > maxErrorDistance)
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
                    ApplyForce(moveBeacons[0], false);
                }
            }
            else if (!allowSlowDown)
            {
                ApplyRotation(moveBeacons[0]);
                ApplyForce(moveBeacons[0], false, false);
            }
            else
            {
                return true;
            }
            return false;
        }

        // Pathfinder
        // Return an alternative position if original cannot be reached
        private Vector3 TestObstacleAround(Vector3 position)
        {
            Vector3 result = position;
            List<Collider> intersectObjects = new List<Collider>(Physics.OverlapBox(position, NavigationCollider.size, transform.rotation));
            intersectObjects.RemoveAll(x => allColliders.Contains(x));
            intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
            intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
            intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
            float nextStepDistance = searchStepDistance;
            bool isDestinationAvaliable = intersectObjects.Count == 0;
            while (nextStepDistance <= searchStepMaxDistance && !isDestinationAvaliable)
            {
                foreach (Vector3 i in UnitVectorHelper.GetSixAroundPoint(position, nextStepDistance))
                {
                    intersectObjects = new List<Collider>(Physics.OverlapBox(i, NavigationCollider.size, transform.rotation));
                    intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                    intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
                    intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
                    if (intersectObjects.Count == 0)
                    {
                        result = i;
                        isDestinationAvaliable = true;
                        break;
                    }
                }
                nextStepDistance += searchStepDistance;
            }
            return result;
        }

        private float TestObstacleInPath(Vector3 from, Vector3 to, float maxDistance = Mathf.Infinity)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Mathf.Min((to - from).magnitude, maxDistance);
            foreach (Vector3 i in agentCorners)
            {
                RaycastHit hit;
                if (Physics.Raycast(from - transform.position + transform.TransformPoint(i), direction, out hit, distance))
                {
                    if (!allColliders.Contains(hit.collider) && hit.collider.GetComponentInParent<RTSGameObjectBaseScript>() != null)
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