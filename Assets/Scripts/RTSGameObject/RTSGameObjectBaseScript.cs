using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject
{
    public class RTSGameObjectBaseScript : MonoBehaviour
    {
        // Used for pathfinder
        public enum ObjectScale
        {
            Ignore,
            Fighter,
            Frigate,
            Cruiser,
            Battleship,
            Mothership,
            Obstacle
        }

        // Represent the type of object
        public enum ObjectType
        {
            Ignore,
            Fighter,
            Frigate,
            Cruiser,
            Battleship,
            Mothership,
            Structure,
            Subsystem,
            Obstacle
        }

        // Set by editor
        [Header("RTS Game Object")]
        [Tooltip("Scale of this object.")]
        public ObjectScale objectScale;
        [Tooltip("Type of this object.")]
        public ObjectType objectType;
        [Tooltip("Type ID, must be unique.")]
        public string typeID;
        [Tooltip("Max HP, HP will be set to this value at begining.")]
        public float maxHP;
        [Tooltip("The collider used in path finding algorithm, it must in layer navigation collider.")]
        public BoxCollider NavigationCollider;
        [Tooltip("The effect played when the object is destroyed.")]
        public GameObject onDestroyedEffect;
        [Header("Information Bar Calculation")]
        [Tooltip("Radius (xz plane) of RTS game object")]
        public float radius;

        // Set when instantiate
        public int Index { get; set; } = -1;
        public int BelongTo { get; set; } = 0;
        public float HP { get; set; }

        protected GameObject lastDamagedBy = null;
        protected GameObject lastRepairedBy = null;

        protected virtual void OnCreatedAction()
        {
            HP = maxHP;
            GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
        }

        protected virtual void OnDestroyedAction()
        {
            GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, lastDamagedBy);
            Destroy(gameObject);
            if (onDestroyedEffect != null)
            {
                Instantiate(onDestroyedEffect, transform.position, new Quaternion());
            }
        }

        public virtual void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
        {
            HP = Mathf.Clamp(HP - damage, 0, maxHP);
            lastDamagedBy = from;
        }

        public virtual void Repair(float amount, float attackPowerRecover, float defencePowerRecover, float movePowerRecover, GameObject from)
        {
            HP = Mathf.Clamp(HP + amount, 0, maxHP);
            lastDamagedBy = from;
        }
    }
}