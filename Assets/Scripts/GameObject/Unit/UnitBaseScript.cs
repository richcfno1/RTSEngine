using System;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability;
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
            public SubsystemBaseScript.SubsystemScale subsystemScale;
            public GameObject subsystem;
        }

        public enum MoveActionType
        {
            Stop,
            Move,
            Rotate,
            ForcedMove
        }

        public struct MoveAction
        {
            public MoveActionType actionType;
            public Vector3 target;
        }

        // Set by editor
        [Header("Unit")]
        [Header("Subsystem")]
        [Tooltip("Subsystems, only anchor data without associated subsystem gameobject in prefab can be set in init data, or aka custome subsystem.\n" +
            "This also means fighter should not have anchor data without subsystem gameobject.")]
        public List<AnchorData> subsyetemAnchors;
        [Header("Modifier")]
        [Tooltip("Similar to HP, which can influence attack.")]
        public float maxAttackPower;
        [Tooltip("Similar to HP, which can influence defence.")]
        public float maxDefencePower;
        [Tooltip("Similar to HP, which can influence move.")]
        public float maxMovePower;

        [Tooltip("Recover rate")]
        [Range(0, 1)]
        public float recoverAttackPower;
        [Tooltip("Recover rate")]
        [Range(0, 1)]
        public float recoverDefencePower;
        [Tooltip("Recover rate")]
        [Range(0, 1)]
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
        public Dictionary<string, AbilityBaseScript> AbilityDictionary { get; private set; } = new Dictionary<string, AbilityBaseScript>();

        protected float curAttackPower;
        protected float curDefencePower;
        protected float curMovePower;

        protected bool enablePathfinder = true;
        protected Vector3 finalPosition;
        protected Vector3 finalRotationTarget;  // Where to look at
        protected List<Vector3> moveBeacons = new List<Vector3>();

        protected Queue<MoveAction> moveActionQueue = new Queue<MoveAction>();

        public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
        {
            curAttackPower = Mathf.Clamp(curAttackPower - attackPowerReduce, 0, maxAttackPower);
            curDefencePower = Mathf.Clamp(curDefencePower - defencePowerReduce, 0, maxDefencePower);
            curMovePower = Mathf.Clamp(curMovePower - movePowerReduce, 0, maxDefencePower);
            base.CreateDamage(damage / DefencePower, attackPowerReduce, defencePowerReduce, movePowerReduce, from);
        }

        public virtual void Stop()
        {
            moveActionQueue.Clear();
        }

        public virtual void AddActionToQueue(MoveAction action)
        {
            moveActionQueue.Enqueue(action);
        }

        public virtual void AddActionToQueue(Queue<MoveAction> actions)
        {
            while (actions.Count > 0)
            {
                moveActionQueue.Enqueue(actions.Dequeue());
            }
        }

        public virtual void SetDestination(Vector3 destination)
        {
            moveActionQueue.Clear();
            moveActionQueue.Enqueue(new MoveAction
            {
                actionType = MoveActionType.Move,
                target = destination
            });
        }

        public virtual void RotateTo(Vector3 target)
        {
            moveActionQueue.Clear();
            moveActionQueue.Enqueue(new MoveAction
            {
                actionType = MoveActionType.Rotate,
                target = target
            });
        }

        public virtual void ForcedMove(Vector3 destination)
        {
            moveActionQueue.Clear();
            moveActionQueue.Enqueue(new MoveAction
            {
                actionType = MoveActionType.ForcedMove,
                target = destination
            });
        }
    }
}