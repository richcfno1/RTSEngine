using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rotation code is written by another author: https://github.com/brihernandez/GunTurrets
public class RailgunTurretScript : AttackSubsystemBaseScript
{
    [Header("Objects")]
    [Tooltip("GameObject used to shoot from the turret")]
    public GameObject bullet;
    [Tooltip("Where should the bullet instantiate")]
    public List<Transform> bulletStartPosition;
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

    private GameObject fireTarget;
    private Vector3 aimPoint;

    private bool aiming = false;
    private bool atRest = false;

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
        if (!Active && HP / maxHP > repairPercentRequired)
        {
            OnSubsystemRepairedAction();
        }
        if (Active)
        {
            if (timer >= coolDown)
            {
                if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                {
                    RaycastHit hit;
                    Vector3 rayPosition = turretBarrels.position;
                    Vector3 rayDirection = turretBarrels.forward;
                    if (Physics.Raycast(rayPosition, rayDirection, out hit, (aimPoint - rayPosition).magnitude))
                    {
                        if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<GameObjectBaseScript>() == null || hit.collider.GetComponent<GameObjectBaseScript>().BelongTo != BelongTo))
                        {
                            Fire();
                            timer = 0;
                        }
                    }
                }
                DetermineFireTarget();
            }
            else
            {
                timer += Time.deltaTime;
            }
            if (fireTarget == null)
            {
                SetIdle(true);
            }
            else
            {
                SetAimpoint(fireTarget.transform.position);
            }
            RotateTurret();
        }
        if (showDebugRay && Active)
        {
            DrawDebugRay();
        }
    }

    protected virtual void Fire()
    {
        foreach (Transform i in bulletStartPosition)
        {
            GameObject temp = Instantiate(bullet, i.position, turretBarrels.rotation);
            BulletBaseScript tempScript = temp.GetComponent<BulletBaseScript>();
            tempScript.moveDirection = turretBarrels.forward;
            tempScript.toIgnore.Add(GetComponent<Collider>());
            tempScript.toIgnore.Add(Parent.GetComponent<Collider>());
            tempScript.createdBy = Parent.gameObject;
        }
    }

    // Try to find a target by the order, compare angleY first, then check obstacles
    private void DetermineFireTarget()
    {
        fireTarget = null;
        foreach (object i in subsystemTarget)
        {
            GameObject temp = (GameObject)i;
            if (temp == null)
            {
                continue;
            }
            Vector3 localTargetPos = turretBase.InverseTransformPoint(temp.transform.position);
            float angleY = Mathf.Asin(Mathf.Abs(localTargetPos.y) / localTargetPos.magnitude);
            if (localTargetPos.y >= 0.0f)
            {
                if ((temp.transform.position - transform.position).magnitude <= lockRange && angleY <= elevation)
                {
                    RaycastHit hit;
                    Vector3 rayPosition = transform.position;
                    Vector3 rayDirection = (temp.transform.position - transform.position).normalized;
                    if (Physics.Raycast(rayPosition, rayDirection, out hit, (temp.transform.position - transform.position).magnitude))
                    {
                        if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<GameObjectBaseScript>() == null || hit.collider.GetComponent<GameObjectBaseScript>().BelongTo != BelongTo))
                        {
                            fireTarget = temp;
                            break;
                        }
                    }
                }
            }
            else
            {
                if ((temp.transform.position - transform.position).magnitude <= lockRange && angleY <= depression)
                {
                    RaycastHit hit;
                    Vector3 rayPosition = transform.position;
                    Vector3 rayDirection = (temp.transform.position - transform.position).normalized;
                    if (Physics.Raycast(rayPosition, rayDirection, out hit, (temp.transform.position - transform.position).magnitude))
                    {
                        if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<GameObjectBaseScript>() == null || hit.collider.GetComponent<GameObjectBaseScript>().BelongTo != BelongTo))
                        {
                            fireTarget = temp;
                            break;
                        }
                    }
                }
            }
        }
        if (fireTarget == null && subsystemTarget.Count != 0 && (GameObject)subsystemTarget[0] != null && 
            (((GameObject)subsystemTarget[0]).transform.position - transform.position).magnitude <= lockRange)
        {
            fireTarget = (GameObject)subsystemTarget[0];
        }
    }
    private void SetAimpoint(Vector3 position)
    {
        aiming = true;
        aimPoint = position;
        atRest = false;
    }


    private void SetIdle(bool idle)
    {
        aiming = !idle;

        if (aiming)
        {
            atRest = false;
        }
    }

    private void RotateTurret()
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

    private void RotateBase()
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

    private void RotateBarrels()
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

    private bool RotateToIdle()
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

    private void DrawDebugRay()
    {
        // DEBUG RAY
        RaycastHit hit;
        Vector3 rayPosition = turretBarrels.position;
        Vector3 rayDirection = turretBarrels.forward;
        if (Physics.Raycast(rayPosition, rayDirection, out hit, (aimPoint - rayPosition).magnitude))
        {
            if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<GameObjectBaseScript>() == null || hit.collider.GetComponent<GameObjectBaseScript>().BelongTo != BelongTo))
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
