using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            get
            {
                var temp = GameManager.GameManagerInstance.GetUnitByIndex(networkLastDamagedBy.Value);
                if (temp != null)
                {
                    return temp.gameObject;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    networkLastDamagedBy.Value = value.GetComponent<RTSGameObjectBaseScript>().Index;
                }
                else
                {
                    networkLastDamagedBy.Value = -1;
                } 
            }
        }
        protected GameObject LastRepairedBy
        {
            get
            {
                var temp = GameManager.GameManagerInstance.GetUnitByIndex(networkLastRepairedBy.Value);
                if (temp != null)
                {
                    return temp.gameObject;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    networkLastRepairedBy.Value = value.GetComponent<RTSGameObjectBaseScript>().Index;
                }
                else
                {
                    networkLastRepairedBy.Value = -1;
                }
            }
        }

        private NetworkVariable<int> networkIndex = new NetworkVariable<int>(-1);
        private NetworkVariable<int> networkBelongTo = new NetworkVariable<int>(0);
        private NetworkVariable<float> networkHP = new NetworkVariable<float>();
        private NetworkVariable<string> networkLuaTag = new NetworkVariable<string>();
        private NetworkVariable<int> networkLastDamagedBy = new NetworkVariable<int>(-1);
        private NetworkVariable<int> networkLastRepairedBy = new NetworkVariable<int>(-1);

        protected NetworkVariable<ulong> networkStartParentID = new NetworkVariable<ulong>(new NetworkVariableSettings
        {
            SendTickrate = -1
        });
        protected NetworkVariable<string> networkStartDirectParentName = new NetworkVariable<string>(new NetworkVariableSettings
        {
            SendTickrate = -1
        });

        // client must destory RTSGO inside other RTSGO to sync with server
        // to achieve this, destroy this RTSGO, but do not call on destroy event
        private bool isClientClearPrefabDestroy = false;

        public void ServerInit(ulong parentID, string directParentName)
        {
            networkStartParentID.Value = parentID;
            networkStartDirectParentName.Value = directParentName;
        }
        // called by Despawn, for client to clear info in game manager
        void OnDestroy()
        {
            if (!isClientClearPrefabDestroy)
            {
                if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
                {
                    GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, LastDamagedBy);
                }
            }
        }

        protected virtual void OnCreatedAction()
        {
            if (Index == -1)
            {
                isClientClearPrefabDestroy = true;
                Destroy(gameObject);
                return;
            }

            // Sync
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                Debug.Log("WOW1!");
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
                                return;
                            }
                        }
                    }
                }
                transform.SetParent(GameManager.GameManagerInstance.masterObject);
            }

            GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                HP = maxHP;
            }
        }

        // Server/Host: called by game logic
        // Client: called by OnDestroy from despawn 
        protected virtual void OnDestroyedAction()
        {
            GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, LastDamagedBy);
            if (onDestroyedEffect != null)
            {
                Instantiate(onDestroyedEffect, transform.position, new Quaternion());
            }
            if (NetworkManager.Singleton.IsServer)
            {
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