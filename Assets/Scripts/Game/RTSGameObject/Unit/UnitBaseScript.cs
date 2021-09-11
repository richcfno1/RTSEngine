using MLAPI;
using System;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.Ability.CommonAbility;
using RTS.Game.Ability.SpecialAbility;
using RTS.Game.RTSGameObject.Subsystem;
using RTS.Game.Helper;
using System.Linq;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;

namespace RTS.Game.RTSGameObject.Unit
{
    public class UnitBaseScript : RTSGameObjectBaseScript
    {
        [Serializable]
        public class AnchorData
        {
            public string anchorName;
            public GameObject anchor;
            public SubsystemBaseScript.SubsystemType subsystemScale;
            public GameObject subsystem;
        }

        public enum FireControlStatus
        {
            Passive,
            Neutral,
            Aggressive
        }

        public enum ActionType
        {
            Stop,  // Stop move
            Move,  // Move to a position. Ship and fighter will act in different ways
            LookAt,  // Head to a position
            LookAtTarget,  // Head to a target
            Follow,  // Follow a gameobject with offset vector
            KeepInRange,  // Similar to follow, but allow unit stay in a range rather than keep exact distance
                          // Also, it will try to find a destination where can "look at target directly without seeing a obstacle"
            KeepInRangeAndLookAt,  // Follow a gameobject with offset and try to head to it. Ship and fighter will act in different ways
            Attack,  // Set attack target
            AttackAndMove,  // Move to a position, but when there ia a enemy nearby, call attack
            UseNoSelectionSpecialAbility,  // Use a special ability
            UseSelectTargetSpecialAbility,  // Use a special ability
            UseSelectSpaceSpecialAbility,  // Use a special ability
            // Deploy called only
            ForcedMove
        }

        public class UnitAction
        {
            public ActionType actionType;
            public List<object> targets;
            /* Targets contains a list of objects which are used to act
             * Move: size = 1: [0] = Vector3 destination
             * LookAt: size = 1: [0] = Vector3 look at
             * LookAtTarget: size = 1: [0] = GameObject look at
             * Follow: size = 2: [0] = GameObject follow to which [1] = Vector3 offset
             * KeepInRange: size = 3: [0] = GameObject follow to which [1] = float upper limitation [2] = float lower limitation
             * KeepInRangeAndLookAt: size = 4: [0] = GameObject follow to which [1] = Vector3 offset (used as direction indicator)
             *                                 [2] = float distance to trigger "follow", when larger than this value, unit will call "follow"
             *                                 [3] = float distance to trigger "headto", when smaller than this value, unit will call "headto"
             * Attack: size = 1: [0] = GameObject target
             * AttackAndMove: size = 1: [0] = Vector3 destination
             * UseNoSelectionSpecialAbility: size = 1: [0] = NoSelectionSpecialAbility
             * UseNoSelectionSpecialAbility: size = 2: [0] = NoSelectionSpecialAbility [1] = GameObject target
             * UseNoSelectionSpecialAbility: size = 2: [0] = NoSelectionSpecialAbility [1] = Vector3 target
             * UseSpecialAbility: Undecided
             * ForcedMove: size = 2: [0] = Vector3 destination [1] = float speed
             */
        }

        // Set by editor
        [Header("Unit")]
        [Header("Subsystem")]
        [Tooltip("Subsystems, only anchor data without associated subsystem gameobject in prefab can be set in init data, or aka custome subsystem.\n" +
            "This also means fighter should not have anchor data without subsystem gameobject.")]
        public List<AnchorData> subsyetemAnchors;

        [Header("Movement")]
        [Tooltip("Multiplier for all forces.")]
        public float forceMultiplier;
        [Tooltip("Force for move.")]
        public float maxForce;
        [Tooltip("Rotation limitation.")]
        public float maxRotationSpeed;
        [Tooltip("Allowed error distance.")]
        public float maxErrorDistance;
        [Tooltip("Allowed error angel.")]
        public float maxErrorAngle;
        [Tooltip("Slowdown distance.")]
        public float slowDownDistance;
        [Tooltip("Lock rotation in Z.")]
        public bool lockRotationZ;


        [Header("Pathfinder")]
        [Tooltip("The radius difference between each search sphere.")]
        public float searchStepDistance;
        [Tooltip("The max radius of search sphere.")]
        public float searchStepMaxDistance;
        [Tooltip("The number of points tested in each sphere.")]
        public float searchMaxRandomNumber;
        [Tooltip("Pathfinder distance.")]
        public float maxDetectDistance;
        [Tooltip("Display debug path trace in game.")]
        public bool displayDebugPath;

        [Header("Vision")]
        [Tooltip("Vision range.")]
        public float visionRange;
        [Tooltip("Vision area ball.")]
        public GameObject visionArea;

        [Header("Fire Control")]
        [Tooltip("Auto engage enemy distance.")]
        public float autoEngageDistance;
        [Tooltip("Time gap between search enemy.")]
        public float autoEngageGap;

        [Header("Modifier")]
        [Tooltip("Similar to HP, which can influence attack.")]
        public float maxAttackPower;
        [Tooltip("Similar to HP, which can influence defence.")]
        public float maxDefencePower;
        [Tooltip("Similar to HP, which can influence move.")]
        public float maxMovePower;
        [Tooltip("Recover")]
        public float recoverAttackPower;
        [Tooltip("Recover")]
        public float recoverDefencePower;
        [Tooltip("Recover")]
        public float recoverMovePower;

        [Header("Build")]
        public float buildPrice;
        public float buildTime;

        // Set when instantiate
        public string UnitTypeID
        {
            get { return networkUnitTypeID.Value; }
            set { networkUnitTypeID.Value = value; }
        }
        // Three basic value which indicate the performance of ability
        public float AttackPowerRatio { get { return AttackPowerValue / maxAttackPower; } set { AttackPowerValue = value * maxAttackPower; } }
        public float DefencePowerRatio { get { return DefencePowerValue / maxDefencePower; } set { DefencePowerValue = value * maxDefencePower; } }
        public float MovePowerRatio { get { return MovePowerValue / maxMovePower; } set { MovePowerValue = value * maxMovePower; } }
        public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();

        public NetworkList<int> VisibleTo
        {
            get { return networkVisibleTo; }
            set { networkVisibleTo = value; }
        }

        public FireControlStatus CurrentFireControlStatus
        {
            get { return networkFireControlStatus.Value; }
            set { networkFireControlStatus.Value = value; }
        }
        public LinkedList<UnitAction> ActionQueue { get; protected set; } = new LinkedList<UnitAction>();

        public MoveAbilityScript MoveAbility { get; set; } = null;
        public AttackAbilityScript AttackAbility { get; set; } = null;
        public CarrierAbilityScript CarrierAbility { get; set; } = null;
        public SortedDictionary<string, List<SpecialAbilityBaseScript>> SpecialAbilityList { get; set; } = new SortedDictionary<string, List<SpecialAbilityBaseScript>>();

        protected float AttackPowerValue
        {
            get { return networkAttackPower.Value; }
            set { networkAttackPower.Value = value; }
        }
        protected float DefencePowerValue
        {
            get { return networkDefencePower.Value; }
            set { networkDefencePower.Value = value; }
        }
        protected float MovePowerValue
        {
            get { return networkMovePower.Value; }
            set { networkMovePower.Value = value; }
        }

        protected GameObject autoEngageTarget = null;

        protected Vector3 finalPosition;
        protected Vector3 finalRotationTarget;  // Where to look at
        protected List<Vector3> moveBeacons = new List<Vector3>();
        protected bool isApproaching = true;

        protected readonly List<Vector3> agentDetectRayStartOffset = new List<Vector3>();
        protected Rigidbody thisBody;
        protected List<Collider> allColliders;
        protected float estimatedMaxSpeed;

        protected float timer = 0;

        private NetworkVariable<string> networkUnitTypeID = new NetworkVariable<string>("");
        private NetworkVariable<float> networkAttackPower = new NetworkVariable<float>(0);
        private NetworkVariable<float> networkDefencePower = new NetworkVariable<float>(0);
        private NetworkVariable<float> networkMovePower = new NetworkVariable<float>(0);
        private NetworkList<int> networkVisibleTo = new NetworkList<int>();
        private NetworkVariable<FireControlStatus> networkFireControlStatus = new NetworkVariable<FireControlStatus>(FireControlStatus.Passive);

        private LineRenderer debugLineRender;

        void Start()
        {
            OnCreatedAction();
            if (displayDebugPath)
            {
                debugLineRender = gameObject.AddComponent<LineRenderer>();
                debugLineRender.startWidth = debugLineRender.endWidth = 5;
            }
            Vector3 min = NavigationCollider.center - NavigationCollider.size * 0.5f;
            Vector3 max = NavigationCollider.center + NavigationCollider.size * 0.5f;
            agentDetectRayStartOffset.Add(Vector3.zero);

            // Detect agent

            agentDetectRayStartOffset.Add(new Vector3(min.x, min.y, min.z));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 0.5f, min.y * 0.5f, min.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 1.1f, min.y * 1.1f, min.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(min.x, min.y, min.z));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 0.5f, min.y * 0.5f, min.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 1.1f, min.y * 1.1f, min.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(min.x, min.y, max.z));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 0.5f, min.y * 0.5f, max.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 1.1f, min.y * 1.1f, max.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(min.x, max.y, min.z));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 0.5f, max.y * 0.5f, min.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 1.1f, max.y * 1.1f, min.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(min.x, max.y, max.z));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 0.5f, max.y * 0.5f, max.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(min.x * 1.1f, max.y * 1.1f, max.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(max.x, min.y, min.z));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 0.5f, min.y * 0.5f, min.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 1.1f, min.y * 1.1f, min.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(max.x, min.y, max.z));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 0.5f, min.y * 0.5f, max.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 1.1f, min.y * 1.1f, max.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(max.x, max.y, min.z));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 0.5f, max.y * 0.5f, min.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 1.1f, max.y * 1.1f, min.z * 1.1f));

            agentDetectRayStartOffset.Add(new Vector3(max.x, max.y, max.z));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 0.5f, max.y * 0.5f, max.z * 0.5f));
            agentDetectRayStartOffset.Add(new Vector3(max.x * 1.1f, max.y * 1.1f, max.z * 1.1f));


            finalPosition = transform.position;
            thisBody = GetComponent<Rigidbody>();
            allColliders = GetComponentsInChildren<Collider>().ToList();
            estimatedMaxSpeed = ((maxForce * forceMultiplier / thisBody.drag) - Time.fixedDeltaTime * maxForce * forceMultiplier) / thisBody.mass;

            radius = NavigationCollider.size.magnitude / 2;
        }

        void Update()
        {
            if (displayDebugPath)
            {
                Vector3[] array = new Vector3[moveBeacons.Count + 1];
                array[0] = transform.position;
                debugLineRender.positionCount = moveBeacons.Count + 1;
                for (int i = 0; i < moveBeacons.Count; i++)
                {
                    array[i + 1] = moveBeacons[i];
                }
                debugLineRender.SetPositions(array);
            }    
        }

        public override void NetworkInitSync()
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer && delayCounter >= networkStartDelayCounter.Value)
            {
                delayCounter = -1;
                foreach (GameObject i in GameManager.GameManagerInstance.GetAllUnits())
                {
                    if (i.GetComponent<NetworkObject>() != null && i.GetComponent<NetworkObject>().NetworkObjectId ==
                        networkStartParentID.Value)
                    {
                        foreach (Transform j in i.GetComponentsInChildren<Transform>())
                        {
                            if (j.gameObject.name == networkStartDirectParentName.Value)
                            {
                                transform.SetParent(j);
                                GameManager.GameManagerInstance.InstantiateClientUnit(gameObject);
                                return;
                            }
                        }
                    }
                }
                transform.SetParent(GameManager.GameManagerInstance.masterObject);
                GameManager.GameManagerInstance.InstantiateClientUnit(gameObject);
            }
        }

        protected override void OnCreatedAction()
        {
            NetworkInitSync();

            GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                HP = maxHP;
                AttackPowerRatio = 1;
                DefencePowerRatio = 1;
                MovePowerRatio = 1;
            }
        }

        protected override void OnDestroyedAction()
        {
            foreach (AnchorData i in subsyetemAnchors)
            {
                if (i.subsystem != null)
                {
                    // Since OnDestroyedAction() of subsystem won't destroy the subsystem and report to GameManager, 
                    // Unit must do it for every subsystem. OnDestroyedAction() of subsystem is used when HP <= 0, 
                    // but subsystem will only be destroyed when unit is destroyed
                    GameManager.GameManagerInstance.OnGameObjectDestroyed(i.subsystem, LastDamagedBy);
                    if (NetworkManager.Singleton.IsServer)
                    {
                        i.subsystem.GetComponent<NetworkObject>().Despawn(true);
                    }
                }
            }
            base.OnDestroyedAction();
        }

        public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
        {
            AttackPowerValue = Mathf.Clamp(AttackPowerValue - attackPowerReduce, 0, maxAttackPower);
            DefencePowerValue = Mathf.Clamp(DefencePowerValue - defencePowerReduce, 0, maxDefencePower);
            MovePowerValue = Mathf.Clamp(MovePowerValue - movePowerReduce, 0, maxDefencePower);
            base.CreateDamage(damage / DefencePowerRatio, attackPowerReduce, defencePowerReduce, movePowerReduce, from);
        }

        public override void Repair(float amount, float attackPowerRecover, float defencePowerRecover, float movePowerRecover, GameObject from)
        {
            AttackPowerValue = Mathf.Clamp(AttackPowerValue + attackPowerRecover, 0, maxAttackPower);
            DefencePowerValue = Mathf.Clamp(DefencePowerValue + defencePowerRecover, 0, maxDefencePower);
            MovePowerValue = Mathf.Clamp(MovePowerValue + movePowerRecover, 0, maxDefencePower);
            base.Repair(amount, attackPowerRecover, defencePowerRecover, movePowerRecover, from);
        }

        public virtual void Stop()
        {
            ActionQueue.Clear();
            moveBeacons.Clear();
            ActionQueue.AddLast(new UnitAction
            {
                actionType = ActionType.Stop,
                targets = new List<object>()
            });
        }

        public virtual void Move(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Move,
                    targets = new List<object>() { destination }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Move,
                    targets = new List<object>() { destination }
                });
            }
        }

        public virtual void LookAt(Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.LookAt,
                    targets = new List<object>() { target }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.LookAt,
                    targets = new List<object>() { target }
                });
            }
        }

        public virtual void LookAtTarget(GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.LookAtTarget,
                    targets = new List<object>() { target }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.LookAtTarget,
                    targets = new List<object>() { target }
                });
            }
        }

        public virtual void Follow(GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            // Distance determination
            float distance = target.GetComponent<RTSGameObjectBaseScript>().radius - radius * 2;
            distance /= 2;
            Vector3 offset = (transform.position - target.transform.position).normalized * distance;
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Follow,
                    targets = new List<object>() { target, offset }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Follow,
                    targets = new List<object>() { target, offset }
                });
            }
        }

        public virtual void Follow(GameObject target, Vector3 offset, bool clearQueue = true, bool addToEnd = true)
        {
            // Distance determination
            float distance = target.GetComponent<RTSGameObjectBaseScript>().radius - radius * 2;
            distance /= 2;
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Follow,
                    targets = new List<object>() { target, offset + offset.normalized * distance }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Follow,
                    targets = new List<object>() { target, offset + offset.normalized * distance }
                });
            }
        }

        public virtual void KeepInRange(GameObject target, float upperBound, float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            // Distance determination
            float distance = target.GetComponent<RTSGameObjectBaseScript>().radius - radius * 2;
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.KeepInRange,
                    targets = new List<object>() { target, upperBound + distance, lowerBound + distance }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.KeepInRange,
                    targets = new List<object>() { target, upperBound + distance, lowerBound + distance }
                });
            }
        }

        public virtual void KeepInRangeAndLookAt(GameObject target, Vector3 offset, float upperBound, 
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
        {
            // Distance determination
            float distance = target.GetComponent<RTSGameObjectBaseScript>().radius - radius * 2;
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.KeepInRangeAndLookAt,
                    targets = new List<object>() { target, offset, upperBound + distance, lowerBound + distance }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.KeepInRangeAndLookAt,
                    targets = new List<object>() { target, offset, upperBound + distance, lowerBound + distance }
                });
            }
        }

        public virtual void Attack(GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Attack,
                    targets = new List<object>() { target }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Attack,
                    targets = new List<object>() { target }
                });
            }
        }

        public virtual void AttackAndMove(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.AttackAndMove,
                    targets = new List<object>() { destination }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.AttackAndMove,
                    targets = new List<object>() { destination }
                });
            }
        }

        public virtual void UseNoSelectionSpecialAbility(NoSelectionSpecialAbilityScript abilities, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.UseNoSelectionSpecialAbility,
                    targets = new List<object>() { abilities }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.UseNoSelectionSpecialAbility,
                    targets = new List<object>() { abilities }
                });
            }
        }

        public virtual void UseSelectTargetSpecialAbility(SelectTargetSpecialAbilityScript abilities, GameObject target, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.UseSelectTargetSpecialAbility,
                    targets = new List<object>() { abilities, target }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.UseSelectTargetSpecialAbility,
                    targets = new List<object>() { abilities, target }
                });
            }
        }

        public virtual void UseSelectSpaceSpecialAbility(SelectSpaceSpecialAbilityScript abilities, Vector3 target, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.UseSelectSpaceSpecialAbility,
                    targets = new List<object>() { abilities, target }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.UseSelectSpaceSpecialAbility,
                    targets = new List<object>() { abilities, target }
                });
            }
        }

        // This function should only be called when deploy the unit
        public virtual void ForcedMove(Vector3 destination, float speed, bool clearQueue = true, bool addToEnd = true)
        {
            if (clearQueue)
            {
                ActionQueue.Clear();
                moveBeacons.Clear();
            }
            if (addToEnd)
            {
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.ForcedMove,
                    targets = new List<object>() { destination, speed }
                });
                ActionQueue.AddLast(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
            }
            else
            {
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.Stop,
                    targets = new List<object>()
                });
                ActionQueue.AddFirst(new UnitAction
                {
                    actionType = ActionType.ForcedMove,
                    targets = new List<object>() { destination, speed }
                });
            }
        }

        // Pathfinder
        // Return an alternative position if original cannot be reached
        protected Vector3 TestObstacleAround(Vector3 position, bool considerObstacleVelocity = true)
        {
            Vector3 result = position;
            List<Collider> intersectObjects = new List<Collider>(Physics.OverlapBox(position, NavigationCollider.size, transform.rotation));
            intersectObjects.RemoveAll(x => allColliders.Contains(x));
            intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
            intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
            intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
            float nextStepDistance = searchStepDistance;
            bool isDestinationAvaliable = intersectObjects.Count == 0;
            if (considerObstacleVelocity && intersectObjects.Count != 0)
            {
                isDestinationAvaliable = true;
                foreach (Collider j in intersectObjects)
                {
                    if (j.GetComponentInParent<Rigidbody>() != null)
                    {
                        if (UnitVectorHelper.CollisionBetwenTwoUnitPath(thisBody.position, estimatedMaxSpeed, radius, position,
                            j.transform.position, j.GetComponentInParent<Rigidbody>().velocity,
                            j.GetComponentInParent<RTSGameObjectBaseScript>().radius))
                        {
                            isDestinationAvaliable = false;
                            break;
                        }
                    }
                }
            }
            while (nextStepDistance <= searchStepMaxDistance && !isDestinationAvaliable)
            {
                foreach (Vector3 i in UnitVectorHelper.GetSixAroundPoint(position, nextStepDistance))
                {
                    intersectObjects = new List<Collider>(Physics.OverlapBox(i, NavigationCollider.size, transform.rotation));
                    intersectObjects.RemoveAll(x => x.CompareTag("Bullet"));
                    intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
                    intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
                    if (considerObstacleVelocity)
                    {
                        bool tempResult = true;
                        foreach (Collider j in intersectObjects)
                        {
                            if (j.GetComponentInParent<Rigidbody>() != null)
                            {
                                if (UnitVectorHelper.CollisionBetwenTwoUnitPath(thisBody.position, estimatedMaxSpeed, radius, position,
                                    j.transform.position, j.GetComponentInParent<Rigidbody>().velocity,
                                    j.GetComponentInParent<RTSGameObjectBaseScript>().radius))
                                {
                                    tempResult = false;
                                    break;
                                }
                            }
                        }
                        if (tempResult)
                        {
                            result = i;
                            isDestinationAvaliable = true;
                            break;
                        }
                    }
                    else
                    {
                        if (intersectObjects.Count == 0)
                        {
                            result = i;
                            isDestinationAvaliable = true;
                            break;
                        }
                    }
                }
                nextStepDistance += searchStepDistance;
            }
            return result;
        }

        protected float TestObstacleInPath(Vector3 from, Vector3 to, float maxDistance = Mathf.Infinity, bool considerObstacleVelocity = true)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Mathf.Min((to - from).magnitude, maxDistance);
            foreach (Vector3 i in agentDetectRayStartOffset)
            {
                RaycastHit hit;
                if (Physics.Raycast(from - transform.position + transform.TransformPoint(i), direction, out hit, distance))
                {
                    if (!allColliders.Contains(hit.collider) && hit.collider.GetComponentInParent<RTSGameObjectBaseScript>() != null)
                    {
                        if (objectScale <= hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().objectScale)
                        {
                            if (considerObstacleVelocity && hit.collider.GetComponentInParent<Rigidbody>() != null)
                            {
                                if (UnitVectorHelper.CollisionBetwenTwoUnitPath(from, estimatedMaxSpeed, radius, to,
                                    hit.collider.transform.position, hit.collider.GetComponentInParent<Rigidbody>().velocity,
                                    hit.collider.GetComponentInParent<RTSGameObjectBaseScript>().radius))
                                {
                                    return (hit.collider.ClosestPoint(from) - from).magnitude;
                                }
                            }
                            else
                            {
                                return (hit.collider.ClosestPoint(from) - from).magnitude;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        protected void FindPath(Vector3 from, Vector3 to)
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
                        intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>() == null);
                        intersectObjects.RemoveAll(x => x.GetComponent<RTSGameObjectBaseScript>().objectScale < objectScale);
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