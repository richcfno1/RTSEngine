using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class TurretBaseScript : AttackSubsystemBaseScript
    {
        [Tooltip("Transform used to provide the horizontal rotation of the turret.")]
        public Transform turretBase;
        [Tooltip("Transform used to provide the vertical rotation of the barrels. Must be a child of the TurretBase.")]
        public Transform turretBarrels;

        [Header("Rotation Limits")]
        [Tooltip("Turn rate of the turret's base and barrels in degrees per second.")]
        public float turnRate = 30.0f;
        [Tooltip("When true, turret rotates according to left/right traverse limits. When false, turret can rotate freely.")]
        public bool limitTraverse = false;
        [Tooltip("When traverse is limited, how many degrees to the left the turret can turn.")]
        [Range(0.0f, 180.0f)]
        public float leftTraverse = 60.0f;
        [Tooltip("When traverse is limited, how many degrees to the right the turret can turn.")]
        [Range(0.0f, 180.0f)]
        public float rightTraverse = 60.0f;
        [Tooltip("How far up the barrel(s) can rotate.")]
        [Range(0.0f, 90.0f)]
        public float elevation = 60.0f;
        [Tooltip("How far down the barrel(s) can rotate.")]
        [Range(0.0f, 90.0f)]
        public float depression = 5.0f;

        [Header("Utilities")]
        [Tooltip("Show the arcs that the turret can aim through.\n\nRed: Left/Right Traverse\nGreen: Elevation\nBlue: Depression")]
        public bool showArcs = false;
        [Tooltip("When game is running in editor, draws a debug ray to show where the turret is aiming.")]
        public bool showDebugRay = true;

        protected Vector3 aimPoint;
        protected bool aiming = false;
        protected bool atRest = false;

        protected void SetAimpoint(Vector3 position)
        {
            aiming = true;
            aimPoint = position;
            atRest = false;
        }

        protected void SetIdle(bool idle)
        {
            aiming = !idle;

            if (aiming)
            {
                atRest = false;
            }
        }

        protected void RotateTurret()
        {
            if (aiming)
            {
                RotateBase();
                RotateBarrels();
            }
            else if (!atRest)
            {
                atRest = RotateToIdle();
            }
        }

        protected void RotateBase()
        {
            // TODO: Turret needs to rotate the long way around if the aimpoint gets behind
            // it and traversal limits prevent it from taking the shortest rotation.
            if (turretBase != null)
            {
                // Note, the local conversion has to come from the parent.
                Vector3 localTargetPos = transform.InverseTransformPoint(aimPoint);
                localTargetPos.y = 0.0f;

                // Clamp target rotation by creating a limited rotation to the target.
                // Use different clamps depending if the target is to the left or right of the turret.
                Vector3 clampedLocalVec2Target = localTargetPos;
                if (limitTraverse)
                {
                    if (localTargetPos.x >= 0.0f)
                        clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * rightTraverse, float.MaxValue);
                    else
                        clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * leftTraverse, float.MaxValue);
                }

                // Create new rotation towards the target in local space.
                Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
                Quaternion newRotation = Quaternion.RotateTowards(turretBase.localRotation, rotationGoal, turnRate * Time.deltaTime);

                // Set the new rotation of the base.
                turretBase.localRotation = newRotation;
            }
        }

        protected void RotateBarrels()
        {
            // TODO: A target position directly to the turret's right will cause the turret
            // to attempt to aim straight up. This looks silly and on slow moving turrets can
            // cause delays on targeting. This is why barrels have a boosted rotation speed.
            if (turretBase != null && turretBarrels != null)
            {
                // Note, the local conversion has to come from the parent.
                Vector3 localTargetPos = turretBase.InverseTransformPoint(aimPoint);
                localTargetPos.x = 0.0f;

                // Clamp target rotation by creating a limited rotation to the target.
                // Use different clamps depending if the target is above or below the turret.
                Vector3 clampedLocalVec2Target = localTargetPos;
                if (localTargetPos.y >= 0.0f)
                    clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * elevation, float.MaxValue);
                else
                    clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * depression, float.MaxValue);

                // Create new rotation towards the target in local space.
                Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
                Quaternion newRotation = Quaternion.RotateTowards(turretBarrels.localRotation, rotationGoal, 2.0f * turnRate * Time.deltaTime);

                // Set the new rotation of the barrels.
                turretBarrels.localRotation = newRotation;
            }
        }

        protected bool RotateToIdle()
        {
            bool baseFinished = false;
            bool barrelsFinished = false;

            if (turretBase != null)
            {
                Quaternion newRotation = Quaternion.RotateTowards(turretBase.localRotation, Quaternion.identity, turnRate * Time.deltaTime);
                turretBase.localRotation = newRotation;

                if (turretBase.localRotation == Quaternion.identity)
                    baseFinished = true;
            }

            if (turretBarrels != null)
            {
                Quaternion newRotation = Quaternion.RotateTowards(turretBarrels.localRotation, Quaternion.identity, 2.0f * turnRate * Time.deltaTime);
                turretBarrels.localRotation = newRotation;

                if (turretBarrels.localRotation == Quaternion.identity)
                    barrelsFinished = true;
            }

            return (baseFinished && barrelsFinished);
        }

        protected void DrawDebugRay()
        {
            // DEBUG RAY
            RaycastHit hit;
            Vector3 rayPosition = turretBarrels.position;
            Vector3 rayDirection = turretBarrels.forward;
            if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange, ~pathfinderLayerMask))
            {
                if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
                {
                    Debug.DrawRay(turretBarrels.position, turretBarrels.forward * lockRange, Color.green);
                }
                else
                {
                    Debug.DrawRay(turretBarrels.position, turretBarrels.forward * lockRange, Color.red);
                }
            }
            else
            {
                Debug.DrawRay(turretBarrels.position, turretBarrels.forward * lockRange, Color.yellow);
            }
        }
    }
}


