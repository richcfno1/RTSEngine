using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserTurretScript : AttackSubsystemBaseScript
{
    [Header("Objects")]
    [Tooltip("LineRenderer of laser")]
    public LineRenderer laserRenderer;
    [Tooltip("Where should the bullet instantiate")]
    public List<Transform> laserStartPosition;
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

    public float laserAppearTime;
    public float damage;
    public float attackPowerReduce;
    public float defencePowerReduce;
    public float movePowerReduce;

    private GameObject fireTarget;
    private Vector3 aimPoint;
    private int laserCount;

    private bool aiming = false;
    private bool atRest = false;

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
        laserCount = 0;
        laserRenderer.enabled = false;
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
            if (fireTarget == null)
            {
                SetIdle(true);
            }
            else
            {
                SetAimpoint(fireTarget.transform.position);
            }
            RotateTurret();
            if (timer >= coolDown / laserStartPosition.Count / Parent.AttackPower)
            {
                if (fireTarget != null && (transform.position - fireTarget.transform.position).magnitude <= lockRange)
                {
                    RaycastHit hit;
                    Vector3 rayPosition = turretBarrels.position;
                    Vector3 rayDirection = turretBarrels.forward;
                    if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange))
                    {
                        if (hit.collider.gameObject == fireTarget)
                        {
                            Fire(laserCount, fireTarget);
                            laserCount++;
                            if (laserCount == laserStartPosition.Count)
                            {
                                laserCount = 0;
                            }
                        }
                    }
                    else
                    {


                    }
                }
                DetermineFireTarget();
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        if (showDebugRay && Active)
        {
            DrawDebugRay();
        }
    }

    protected virtual void Fire(int laserIndex, GameObject hit)
    {
        laserRenderer.enabled = true;
        laserRenderer.SetPositions(new Vector3[] { laserStartPosition[laserIndex].position, hit.transform.position });
        hit.GetComponent<RTSGameObjectBaseScript>().CreateDamage(damage, attackPowerReduce, defencePowerReduce, movePowerReduce, Parent.gameObject);
        StopAllCoroutines();
        StartCoroutine(WaitAndPrint(laserAppearTime));
    }
    private IEnumerator WaitAndPrint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        laserRenderer.enabled = false;
    }

    // Try to find a target by the order, compare angleY first, then check obstacles
    protected override void DetermineFireTarget()
    {
        if (subsystemTarget == null)
        {
            return;
        }
        if (subsystemTarget.Count == 1 && possibleTargetTags.Contains(((GameObject)subsystemTarget[0]).tag))
        {
            GameObject target = (GameObject)subsystemTarget[0];
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit, (target.transform.position - transform.position).magnitude))
            {
                if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
                {
                    fireTarget = target;
                    return;
                }
            }
        }
        List<Collider> allPossibleTargets = new List<Collider>(Physics.OverlapSphere(transform.position, lockRange));
        List<Collider> filteredPossibleTargets = new List<Collider>();
        foreach (string i in possibleTargetTags)
        {
            filteredPossibleTargets.AddRange(allPossibleTargets.Where(x => x.CompareTag(i)));
        }
        filteredPossibleTargets = filteredPossibleTargets.Where(x => x.GetComponent<RTSGameObjectBaseScript>() != null &&
            x.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo).ToList();
        filteredPossibleTargets.Sort((x, y) => Comparer.Default.Compare(
            (x.transform.position - transform.position).sqrMagnitude, (y.transform.position - transform.position).sqrMagnitude));
        foreach (Collider i in filteredPossibleTargets)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (i.transform.position - transform.position).normalized, out hit, (i.transform.position - transform.position).magnitude))
            {
                if (hit.collider.tag != "AimCollider" && (hit.collider.GetComponent<RTSGameObjectBaseScript>() == null || hit.collider.GetComponent<RTSGameObjectBaseScript>().BelongTo != BelongTo))
                {
                    fireTarget = i.gameObject;
                    return;
                }
            }
        }
        fireTarget = null;
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
        if (Physics.Raycast(rayPosition, rayDirection, out hit, lockRange))
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
