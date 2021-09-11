using MLAPI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.Ability.SpecialAbility;
using RTS.Game.RTSGameObject.Subsystem;

namespace RTS.Game.RTSGameObject.Unit
{
    public class FighterBaseScript : UnitBaseScript
    {
        // Update is called once per frame
        void FixedUpdate()
        {
            // Vision
            if (visionArea != null && BelongTo == GameManager.GameManagerInstance.SelfIndex)
            {
                visionArea.transform.localScale = new Vector3(visionRange, visionRange, visionRange) * 2;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                if (delayCounter != -1)
                {
                    NetworkInitSync();
                }
                return;
            }

            SetSeed();
            if (HP <= 0)
            {
                OnDestroyedAction();
                return;
            }
            if (lockRotationZ)
            {
                thisBody.MoveRotation(Quaternion.Euler(thisBody.rotation.eulerAngles.x, thisBody.rotation.eulerAngles.y, 0));
            }

            // Recover
            AttackPowerRatio = Mathf.Clamp01(AttackPowerRatio + recoverAttackPower * Time.fixedDeltaTime);
            DefencePowerRatio = Mathf.Clamp01(DefencePowerRatio + recoverDefencePower * Time.fixedDeltaTime);
            MovePowerRatio = Mathf.Clamp01(MovePowerRatio + recoverMovePower * Time.fixedDeltaTime);

            // Action
            timer += Time.fixedDeltaTime;
            // In aggressive status, when detect a nearby enemy, call attack ability
            if (timer >= autoEngageGap && AttackAbility != null && CurrentFireControlStatus == FireControlStatus.Aggressive && autoEngageTarget == null &&
                (ActionQueue.Count == 0 || ActionQueue.First().actionType != ActionType.ForcedMove))
            {
                GameObject temp = null;
                foreach (GameObject i in GameManager.GameManagerInstance.enemyUnitsTable[BelongTo])
                {
                    if ((i.transform.position - transform.position).magnitude <= autoEngageDistance)
                    {
                        temp = i;
                        break;
                    }
                }
                if (temp != null)
                {
                    Attack(temp);
                    autoEngageTarget = temp;
                }
                timer = 0;
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
                            FindPath(thisBody.position, finalPosition);
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

                            for (int i = 0; i < searchMaxRandomNumber; i++)
                            {
                                if (Physics.RaycastAll(finalPosition, (keepInRangeAndHeadToTarget.transform.position - finalPosition).normalized,
                                    (keepInRangeAndHeadToTarget.transform.position - finalPosition).magnitude).
                                    Where(x =>
                                    {
                                        if (keepInRangeAndHeadToTarget.GetComponent<SubsystemBaseScript>() != null)
                                        {
                                            return !transform.GetComponentsInChildren<Collider>().Contains(x.collider) &&
                                            !keepInRangeAndHeadToTarget.GetComponent<SubsystemBaseScript>().Host.transform.GetComponentsInChildren<Collider>().Contains(x.collider);
                                        }
                                        else
                                        {
                                            return !transform.GetComponentsInChildren<Collider>().Contains(x.collider) &&
                                            !keepInRangeAndHeadToTarget.transform.GetComponentsInChildren<Collider>().Contains(x.collider);
                                        }
                                    }).ToList().Count == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    finalPosition = keepInRangeAndHeadToTarget.transform.position +
                                        new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized *
                                        (isApproaching ? (float)action.targets[3] : (float)action.targets[2]);
                                }
                            }
                            FindPath(thisBody.position, finalPosition);
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
                            if (timer >= autoEngageGap && AttackAbility != null && autoEngageTarget == null)
                            {
                                GameObject temp = null;
                                foreach (GameObject i in GameManager.GameManagerInstance.enemyUnitsTable[BelongTo])
                                {
                                    if ((i.transform.position - transform.position).magnitude <= autoEngageDistance)
                                    {
                                        temp = i;
                                        break;
                                    }
                                }
                                if (temp != null)
                                {
                                    Attack(temp);
                                    autoEngageTarget = temp;
                                }
                                timer = 0;
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
                            float moveDistance = speed * Time.fixedDeltaTime * MovePowerRatio;

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
            thisBody.MoveRotation(Quaternion.RotateTowards(thisBody.rotation, Quaternion.LookRotation(rotateDirection), Time.fixedDeltaTime * maxRotationSpeed));
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
    }
}