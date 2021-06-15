using Neo.IronLua;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public string baseTypeName;
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
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
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
        public float playerMoney;
        public List<int> playerGameObjects = new List<int>();
    }

    public static GameManager GameManagerInstance { get; private set; }

    public int selfIndex;
    public TextAsset debugInitDataAsset;

    public TextAsset gameObjectLibraryAsset;
    public TextAsset abilityLibraryAsset;
    public Dictionary<string, string> gameObjectLibrary = new Dictionary<string, string>();
    public Dictionary<string, string> abilityLibrary = new Dictionary<string, string>();
    public Dictionary<string, UnitLibraryData> unitLibrary = new Dictionary<string, UnitLibraryData>();

    private int gameObjectIndexCounter = 0;
    private Dictionary<int, Player> allPlayers = new Dictionary<int, Player>();
    private Dictionary<int, GameObject> allGameObjectsDict = new Dictionary<int, GameObject>();
    private List<GameObject> allGameObjectsList = new List<GameObject>();
    Lua gameLua = new Lua();

    void Awake()
    {
        GameManagerInstance = this;
        gameObjectLibrary = JsonConvert.DeserializeObject<Dictionary<string, string>>(gameObjectLibraryAsset.text);
        abilityLibrary = JsonConvert.DeserializeObject<Dictionary<string, string>>(abilityLibraryAsset.text);

        if (debugInitDataAsset != null)
        {
            GameInitData initData = JsonConvert.DeserializeObject<GameInitData>(debugInitDataAsset.text);
            InitFromInitData(initData);
        }
    }

    private float recordData = 0;
    private float recordData2 = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Debug use
        if (Input.GetKeyDown(KeyCode.O))
        {
            InstantiateUnit("StandardFrigate", new Vector3(recordData, 0, recordData + recordData2), new Quaternion(), GameObject.Find("GameObject").transform, 0);
            recordData -= 25;
            Debug.Log(recordData / 25);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            recordData = 0;
            recordData2 = -25;
        }
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

    public GameObject InstantiateUnit(string unitType, Vector3 position, Quaternion rotation, Transform parent, int belongTo)
    {
        if (!unitLibrary.ContainsKey(unitType))
        {
            Debug.LogError("No such unit: " + unitType);
            return null;
        }
        UnitLibraryData libraryData = unitLibrary[unitType];

        // Ship
        GameObject result = Instantiate(Resources.Load<GameObject>(gameObjectLibrary[libraryData.baseTypeName]), position, rotation, parent);
        if (result.GetComponent<ShipBaseScript>() != null)
        {
            ShipBaseScript shipScript = result.GetComponent<ShipBaseScript>();
            shipScript.UnitTypeID = unitType;
            shipScript.PropertyDictionary = libraryData.properties;

            // Init subsystem
            foreach (UnitBaseScript.AnchorData anchorData in shipScript.subsyetemAnchors)
            {
                // Already have a subsystem (set in prefab)
                if (anchorData.subsystem != null)
                {
                    anchorData.subsystem.GetComponent<SubsystemBaseScript>().Host = shipScript;
                }
                else if (libraryData.subsystems.ContainsKey(anchorData.anchorName) && anchorData.subsystem == null)
                {
                    string subsystemTypeName = libraryData.subsystems[anchorData.anchorName];
                    GameObject temp = Instantiate(Resources.Load<GameObject>(gameObjectLibrary[subsystemTypeName]), anchorData.anchor.transform);
                    SubsystemBaseScript subsystemScript = temp.GetComponent<SubsystemBaseScript>();
                    if (anchorData.subsystemScale != subsystemScript.scale)
                    {
                        Debug.Log("Subsystem mismatch with anchor type: " + anchorData.subsystemScale + " and " + subsystemScript.scale);
                    }
                    else
                    {
                        anchorData.subsystem = temp;
                        subsystemScript.Host = shipScript;
                    }
                }
            }

            // Init ability
            foreach (KeyValuePair<string, List<string>> ability in libraryData.abilities)
            {
                Type abilityType = Type.GetType(abilityLibrary[ability.Key]);
                AbilityBaseScript abilityScript = (AbilityBaseScript)result.AddComponent(abilityType);
                foreach (string supportedSubsystemAnchor in ability.Value)
                {
                    if (supportedSubsystemAnchor != libraryData.baseTypeName)
                    {
                        GameObject temp = shipScript.subsyetemAnchors.FirstOrDefault(x => x.anchorName == supportedSubsystemAnchor).subsystem;
                        if (temp == default)
                        {
                            Debug.LogError("Cannot find subsystem: " + supportedSubsystemAnchor);
                        }
                        else
                        {
                            if (temp.GetComponent<SubsystemBaseScript>().supportedAbility.Contains((AbilityBaseScript.AbilityType)Enum.Parse(typeof(AbilityBaseScript.AbilityType), ability.Key)))
                            {
                                abilityScript.SupportedBy.Add(temp.GetComponent<SubsystemBaseScript>());
                            }
                            else
                            {
                                Debug.LogError("Subsystem cannot support ability: " + ability.Key);
                            }
                        }
                    }
                }
                abilityScript.Host = shipScript;
                shipScript.AbilityDictionary.Add(ability.Key, abilityScript);
            }
        }

        // Fighter
        else if (result.GetComponent<FighterBaseScript>() != null)
        {
            FighterBaseScript fighterScript = result.GetComponent<FighterBaseScript>();
            fighterScript.UnitTypeID = unitType;
            fighterScript.PropertyDictionary = libraryData.properties;

            // Init ability
            foreach (KeyValuePair<string, List<string>> ability in libraryData.abilities)
            {
                Type abilityType = Type.GetType(abilityLibrary[ability.Key]);
                AbilityBaseScript abilityScript = (AbilityBaseScript)result.AddComponent(abilityType);
                foreach (string supportedSubsystemAnchor in ability.Value)
                {
                    if (supportedSubsystemAnchor != libraryData.baseTypeName)
                    {
                        // Haha, maybe I need change the rule
                        Debug.LogError("Fighter does not have any subsystem: " + supportedSubsystemAnchor);
                    }
                }
                abilityScript.Host = fighterScript;
                fighterScript.AbilityDictionary.Add(ability.Key, abilityScript);
            }
        }

        // Set belonging
        result.GetComponent<RTSGameObjectBaseScript>().BelongTo = belongTo;
        foreach (RTSGameObjectBaseScript i in result.GetComponentsInChildren<RTSGameObjectBaseScript>())
        {
            i.BelongTo = belongTo;
        }
        return result;
    }

    public void OnGameObjectCreated(GameObject self)
    {
        // TODO: LUA

        // Index
        self.GetComponent<RTSGameObjectBaseScript>().Index = gameObjectIndexCounter;
        allGameObjectsDict.Add(gameObjectIndexCounter, self);
        allGameObjectsList.Add(self);

        // Player
        int playerIndex = self.GetComponent<RTSGameObjectBaseScript>().BelongTo;
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
        int gameObjectIndex = self.GetComponent<RTSGameObjectBaseScript>().Index;
        allGameObjectsDict.Remove(gameObjectIndex);
        allGameObjectsList.Remove(self);

        // Player
        int playerIndex = self.GetComponent<RTSGameObjectBaseScript>().BelongTo;
        allPlayers[playerIndex].playerGameObjects.Remove(gameObjectIndex);
    }

    public ref List<GameObject> GetAllGameObjects()
    {
        return ref allGameObjectsList;
    }
}
