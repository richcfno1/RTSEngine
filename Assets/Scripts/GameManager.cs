using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.IronLua;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public struct PlayerData
    {
        public int index;
        public string name;
    }

    public struct UnitLibraryData
    {
        public string unitTypeName;
        public string shipTypeName;
        public Dictionary<string, float> properties;
        // string1 = anchor name, string2 = subsystem type
        public Dictionary<string, string> subsystems;
        // string = ability type, List<string> = supported by which anchor (or use shipTypeName to indicate supported by ship)
        public Dictionary<string, List<string>> abilities;
    }

    public struct UnitData
    {
        public string type;
        public int belongTo;
        public Vector3 position;
        public Quaternion rotation;
    }

    public struct GameInitData
    {
        public List<PlayerData> initPlayerData;
        public List<UnitLibraryData> initUnitLibraryData;
        public List<UnitData> initUnitData;
    }

    private class Player
    {
        public string playerName;
        public List<int> playerGameObjects = new List<int>();
    }

    public static GameManager GameManagerInstance { get; private set; }

    private int gameObjectIndexCounter = 0;
    private Dictionary<int, Player> allPlayers = new Dictionary<int, Player>();
    private Dictionary<int, GameObject> allGameObjects = new Dictionary<int, GameObject>();
    private Dictionary<string, UnitLibraryData> unitLibrary = new Dictionary<string, UnitLibraryData>();
    Lua gameLua = new Lua();

    void Awake()
    {
        GameManagerInstance = this; 
        InitFromInitData(new GameInitData()
        {
            initPlayerData = new List<PlayerData>()
            {
                new PlayerData()
                {
                    index = 0,
                    name = "Nature"
                },
                new PlayerData()
                {
                    index = 1,
                    name = "RC"
                }
            },
            initUnitLibraryData = new List<UnitLibraryData>()
            {
                new UnitLibraryData()
                {
                    unitTypeName = "StandardFrigate",
                    shipTypeName = "Frigate1",
                    // Move
                    //public float agentMoveSpeed;
                    //public float agentRotateSpeed;
                    //public float agentAccelerateLimit;

                    //// Search
                    //public float agentRadius;
                    //public float searchStepDistance;
                    //public float searchStepMaxDistance;
                    //public int searchMaxRandomNumber;
                    properties = new Dictionary<string, float>()
                    {
                        { "MoveSpeed", 20 },
                        { "RotateSpeed", 20 },
                        { "AccelerateLimit", 0.5f },
                        { "MoveAgentRadius", 15 },
                        { "MoveSearchStepDistance", 5 },
                        { "MoveSearchStepLimit", 100 },
                        { "MoveSearchRandomNumber", 20 }
                    },
                    subsystems = new Dictionary<string, string>()
                    {
                        { "TurretAnchor1", "Turret1"}
                    },
                    abilities = new Dictionary<string, List<string>>()
                    {
                        { "Move", new List<string>(){ "Frigate1" } }
                    }
                }
            },
            initUnitData = new List<UnitData>()
            {
                new UnitData()
                {
                    type = "StandardFrigate",
                    belongTo = 0,
                    position = new Vector3(),
                    rotation = new Quaternion()
                }
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitFromInitData(GameInitData data)
    {
        // Player
        foreach (PlayerData i in data.initPlayerData)
        {
            allPlayers.Add(i.index, new Player()
            {
                playerName = i.name,
                playerGameObjects = new List<int>()
            });
        }

        // UnitLibary
        foreach (UnitLibraryData i in data.initUnitLibraryData)
        {
            unitLibrary.Add(i.unitTypeName, i);
        }

        // Instantiate units
        foreach (UnitData i in data.initUnitData)
        {
            if (unitLibrary.ContainsKey(i.type))
            {
                InstantiateUnit(i.type, i.position, i.rotation, GameObject.Find("GameObject").transform, i.belongTo);
            }
            else
            {
                Debug.Log("Cannot find unit type" + i.type);
            }
        }
    }

    public void InstantiateUnit(string unitType, Vector3 position, Quaternion rotation, Transform parent, int belongTo)
    {
        if (!unitLibrary.ContainsKey(unitType))
        {
            Debug.LogError("No such unit: " + unitType);
            return;
        }

        Dictionary<string, string> shipLibrary = new Dictionary<string, string>();
        shipLibrary.Add("Frigate1", "GameObject/Ship/Frigate1");
        Dictionary<string, string> subsystemLibrary = new Dictionary<string, string>();
        subsystemLibrary.Add("Turret1", "GameObject/Subsystem/Turret1");
        Dictionary<string, string> abilityLibrary = new Dictionary<string, string>();
        abilityLibrary.Add("Move", "MoveAbilityScript");
        UnitLibraryData libraryData = unitLibrary[unitType];

        GameObject result = Instantiate(Resources.Load<GameObject>(shipLibrary[libraryData.shipTypeName]), GameObject.Find("GameObject").transform);
        ShipBaseScript shipScript = result.GetComponent<ShipBaseScript>();
        shipScript.PropertyDictionary = libraryData.properties;
        foreach (ShipBaseScript.AnchorData anchorData in shipScript.subsyetemAnchors)
        {
            if (libraryData.subsystems.ContainsKey(anchorData.anchorName))
            {
                string subsystemTypeName = libraryData.subsystems[anchorData.anchorName];
                GameObject temp = Instantiate(Resources.Load<GameObject>(subsystemLibrary[subsystemTypeName]), anchorData.anchor.transform);
                SubsystemBaseScript subsystemScript = temp.GetComponent<SubsystemBaseScript>();
                if (anchorData.subsystemScale != subsystemScript.scale)
                {
                    Debug.Log("Subsystem mismatch with anchor type: " + anchorData.subsystemScale + " and " + subsystemScript.scale);
                }
                else
                {
                    shipScript.SubsystemDictionary.Add(temp, anchorData.anchor);
                    subsystemScript.Parent = shipScript;
                }
            }
        }
        foreach (KeyValuePair<string, List<string>> ability in libraryData.abilities)
        {
            Type abilityType = Type.GetType(abilityLibrary[ability.Key] + ",Assembly-CSharp");
            AbilityBaseScript abilityScript = (AbilityBaseScript)result.AddComponent(abilityType);
            foreach (string supportedSubsystemAnchor in ability.Value)
            {
                if (supportedSubsystemAnchor != libraryData.shipTypeName)
                {
                    GameObject temp = shipScript.SubsystemDictionary.FirstOrDefault(x => x.Value.name == supportedSubsystemAnchor).Key;
                    abilityScript.SupportedBy.Add(temp.GetComponent<SubsystemBaseScript>());
                }
            }
            abilityScript.Parent = shipScript;
            shipScript.AbilityDictionary.Add(ability.Key, abilityScript);
        }

        // Set belonging
        result.GetComponent<GameObjectBaseScript>().BelongTo = belongTo;
        foreach (GameObjectBaseScript i in result.GetComponentsInChildren<GameObjectBaseScript>())
        {
            i.BelongTo = belongTo;
        }
    }

    public void OnGameObjectCreated(GameObject self)
    {
        // TODO: LUA

        // Index
        self.GetComponent<GameObjectBaseScript>().Index = gameObjectIndexCounter;
        allGameObjects.Add(gameObjectIndexCounter, self);

        // Player
        int playerIndex = self.GetComponent<GameObjectBaseScript>().BelongTo;
        allPlayers[playerIndex].playerGameObjects.Add(gameObjectIndexCounter);

        gameObjectIndexCounter++;
    }

    public void OnGameObjectDamaged(GameObject self, GameObject other)
    {
        // TODO: LUA

    }

    public void OnGameObjectDestroyed(GameObject self, GameObject other)
    {
        // TODO: LUA

        // Index
        int gameObjectIndex = self.GetComponent<GameObjectBaseScript>().Index;
        allGameObjects.Remove(gameObjectIndex);

        // Player
        int playerIndex = self.GetComponent<GameObjectBaseScript>().BelongTo;
        allPlayers[playerIndex].playerGameObjects.Remove(gameObjectIndex);
    }

    public List<GameObject> GetAllGameObjects()
    {
        return new List<GameObject>(allGameObjects.Values);
    }
}
