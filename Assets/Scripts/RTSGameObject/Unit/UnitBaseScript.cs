using System;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability.CommonAbility;
using RTS.Ability.SpecialAbility;
using RTS.RTSGameObject.Subsystem;

namespace RTS.RTSGameObject.Unit
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
            UseSpecialAbility,  // Use a special ability, NOT IMPLEMENTED!
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
             * UseSpecialAbility: Undecided
             */
        }

        // Set by editor
        [Header("Unit")]
        [Header("Subsystem")]
        [Tooltip("Subsystems, only anchor data without associated subsystem gameobject in prefab can be set in init data, or aka custome subsystem.\n" +
            "This also means fighter should not have anchor data without subsystem gameobject.")]
        public List<AnchorData> subsyetemAnchors;

        [Header("Path finder")]
        [Tooltip("The radius difference between each search sphere.")]
        public float searchStepDistance;
        [Tooltip("The max radius of search sphere.")]
        public float searchStepMaxDistance;
        [Tooltip("The number of points tested in each sphere.")]
        public float searchMaxRandomNumber;

        [Header("Fire Control")]
        public float autoEngageDistance;

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
        public string UnitTypeID { get; set; }
        // Three basic value which indicate the performance of ability
        public float AttackPower { get { return curAttackPower / maxAttackPower; } set { curAttackPower = value * maxAttackPower; } }
        public float DefencePower { get { return curDefencePower / maxDefencePower; } set { curDefencePower = value * maxDefencePower; } }
        public float MovePower { get { return curMovePower / maxMovePower; } set { curMovePower = value * maxMovePower; } }
        public Dictionary<string, float> PropertyDictionary { get; set; } = new Dictionary<string, float>();
        public FireControlStatus CurrentFireControlStatus { get; set; } = FireControlStatus.Neutral;
        public LinkedList<UnitAction> ActionQueue { get; protected set; } = new LinkedList<UnitAction>();

        public MoveAbilityScript MoveAbility { get; set; } = null;
        public AttackAbilityScript AttackAbility { get; set; } = null;
        public CarrierAbilityScript CarrierAbility { get; set; } = null;
        public SortedDictionary<string, List<SpecialAbilityBaseScript>> SpecialAbilityList { get; set; } = new SortedDictionary<string, List<SpecialAbilityBaseScript>>();

        protected float curAttackPower;
        protected float curDefencePower;
        protected float curMovePower;

        protected GameObject autoEngageTarget = null;

        protected Vector3 finalPosition;
        protected Vector3 finalRotationTarget;  // Where to look at
        protected List<Vector3> moveBeacons = new List<Vector3>();
        protected bool isApproaching = true;

        public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
        {
            curAttackPower = Mathf.Clamp(curAttackPower - attackPowerReduce, 0, maxAttackPower);
            curDefencePower = Mathf.Clamp(curDefencePower - defencePowerReduce, 0, maxDefencePower);
            curMovePower = Mathf.Clamp(curMovePower - movePowerReduce, 0, maxDefencePower);
            base.CreateDamage(damage / DefencePower, attackPowerReduce, defencePowerReduce, movePowerReduce, from);
        }

        public override void Repair(float amount, float attackPowerRecover, float defencePowerRecover, float movePowerRecover, GameObject from)
        {
            curAttackPower = Mathf.Clamp(curAttackPower + attackPowerRecover, 0, maxAttackPower);
            curDefencePower = Mathf.Clamp(curDefencePower + defencePowerRecover, 0, maxDefencePower);
            curMovePower = Mathf.Clamp(curMovePower + movePowerRecover, 0, maxDefencePower);
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
            float distance = target.GetComponent<Collider>().bounds.size.magnitude + GetComponent<Collider>().bounds.size.magnitude;
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
            float distance = target.GetComponent<Collider>().bounds.size.magnitude + GetComponent<Collider>().bounds.size.magnitude;
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
                    targets = new List<object>() { target, upperBound, lowerBound }
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
                    targets = new List<object>() { target, upperBound, lowerBound }
                });
            }
        }

        public virtual void KeepInRangeAndLookAt(GameObject target, Vector3 offset, float upperBound, 
            float lowerBound, bool clearQueue = true, bool addToEnd = true)
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
                    actionType = ActionType.KeepInRangeAndLookAt,
                    targets = new List<object>() { target, offset, upperBound, lowerBound }
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
                    targets = new List<object>() { target, offset, upperBound, lowerBound }
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

        // This function should only be called when deploy the unit
        public virtual void ForcedMove(Vector3 destination, bool clearQueue = true, bool addToEnd = true)
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
                    actionType = ActionType.ForcedMove,
                    targets = new List<object>() { destination }
                });
            }
        }
    }
}