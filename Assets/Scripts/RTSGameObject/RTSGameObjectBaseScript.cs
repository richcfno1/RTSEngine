using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject
{
    public class RTSGameObjectBaseScript : NetworkBehaviour
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
        [Tooltip("Radius (xz plane) of RTS game object. If this is a unit, recalculate at init because pathfinding will replay on it.")]
        public float radius;

        // Set when instantiate
        public int Index
        {
            get { return networkIndex.Value; }
            set { networkIndex.Value = value; }
        }
        public int BelongTo
        {
            get { return networkBelongTo.Value; }
            set { networkBelongTo.Value = value; }
        }
        public float HP
        {
            get { return networkHP.Value; }
            set { networkHP.Value = value; }
        }
        public string LuaTag
        {
            get { return networkLuaTag.Value; }
            set { networkLuaTag.Value = value; }
        }

        protected GameObject LastDamagedBy
        {
            get { return networkLastDamagedBy.Value; }
            set { networkLastDamagedBy.Value = value; }
        }
        protected GameObject LastRepairedBy
        {
            get { return networkLastRepairedBy.Value; }
            set { networkLastRepairedBy.Value = value; }
        }

        private NetworkVariable<int> networkIndex = new NetworkVariable<int>(-1);
        private NetworkVariable<int> networkBelongTo = new NetworkVariable<int>(0);
        private NetworkVariable<float> networkHP = new NetworkVariable<float>();
        private NetworkVariable<string> networkLuaTag = new NetworkVariable<string>();

        private NetworkVariable<GameObject> networkLastDamagedBy = new NetworkVariable<GameObject>();
        private NetworkVariable<GameObject> networkLastRepairedBy = new NetworkVariable<GameObject>();

        private void OnNetworkInstantiate()
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                OnCreatedAction();
            }
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                OnDestroyedAction();
            }
        }

        protected virtual void OnCreatedAction()
        {
            GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
            if (NetworkManager.Singleton.IsServer)
            {
                HP = maxHP;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }

        protected virtual void OnDestroyedAction()
        {
            GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, LastDamagedBy);
            if (NetworkManager.Singleton.IsServer)
            {
                if (onDestroyedEffect != null)
                {
                    Instantiate(onDestroyedEffect, transform.position, new Quaternion());
                }
                gameObject.GetComponent<NetworkObject>().Despawn();
                Destroy(gameObject);
            }
        }

        public virtual void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
        {
            GameManager.GameManagerInstance.OnGameObjectDamaged(gameObject, from);
            if (NetworkManager.Singleton.IsServer)
            {
                HP = Mathf.Clamp(HP - damage, 0, maxHP);
                LastDamagedBy = from;
            }
        }

        public virtual void Repair(float amount, float attackPowerRecover, float defencePowerRecover, float movePowerRecover, GameObject from)
        {
            GameManager.GameManagerInstance.OnGameObjectRepaired(gameObject, from);
            if (NetworkManager.Singleton.IsServer)
            {
                HP = Mathf.Clamp(HP + amount, 0, maxHP);
                LastDamagedBy = from;
            }
        }

        // Must call this function if using random in fixedupdate
        // For frame synchronization
        protected void SetSeed()
        {
            Random.InitState(GameManager.GameManagerInstance.FrameCount + Index);
        }
    }
}