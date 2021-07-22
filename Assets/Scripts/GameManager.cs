using Neo.IronLua;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.Ability.CommonAbility;
using RTS.Ability.SpecialAbility;
using RTS.Helper;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using RTS.RTSGameObject.Subsystem;

namespace RTS
{
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
            public Dictionary<string, List<string>> commonAbilities;
            public Dictionary<string, List<string>> specialAbilities;
            // not particularly useful, it's just here to show how it could be used
            public Dictionary<string, string> scripts;
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
        public ScriptSystem ScriptSystem { get; private set; }

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

        void Awake()
        {
            GameManagerInstance = this;
            ScriptSystem = new ScriptSystem();
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
                InstantiateUnit("StandardFrigate", new Vector3(recordData, 0, recordData + recordData2), new Quaternion(), GameObject.Find("RTSGameObject").transform, 0);
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
                    InstantiateUnit(i.type, i.position, i.rotation, GameObject.Find("RTSGameObject").transform, i.belongTo);
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

            // debug script:
            if (libraryData.scripts != null && libraryData.scripts.TryGetValue("Test", out var code)) {
                var script = ScriptSystem.CreateScript("Test", code);
                ScriptSystem.ExecuteScript(script);
            }

            // Ship
            GameObject result = Instantiate(Resources.Load<GameObject>(gameObjectLibrary[libraryData.baseTypeName]), position, rotation, parent);
            if (result.GetComponent<UnitBaseScript>() != null)
            {
                UnitBaseScript shipScript = result.GetComponent<UnitBaseScript>();
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
                        if (anchorData.subsystemScale != subsystemScript.type)
                        {
                            Debug.Log("Subsystem mismatch with anchor type: " + anchorData.subsystemScale + " and " + subsystemScript.type);
                        }
                        else
                        {
                            anchorData.subsystem = temp;
                            subsystemScript.Host = shipScript;
                        }
                    }
                }

                // Init common ability
                foreach (KeyValuePair<string, List<string>> ability in libraryData.commonAbilities)
                {
                    if (!abilityLibrary.ContainsKey(ability.Key))
                    {
                        Debug.LogError("Cannot find ability: " + ability.Key);
                        continue;
                    }
                    Type abilityType = Type.GetType(abilityLibrary[ability.Key]);
                    CommonAbilityBaseScript abilityScript = (CommonAbilityBaseScript)result.AddComponent(abilityType);
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
                                if (temp.GetComponent<SubsystemBaseScript>().supportedCommonAbility.Contains(
                                    (CommonAbilityBaseScript.CommonAbilityType)Enum.Parse(typeof(CommonAbilityBaseScript.CommonAbilityType), ability.Key)))
                                {
                                    abilityScript.SupportedBy.Add(temp.GetComponent<SubsystemBaseScript>());
                                }
                                else
                                {
                                    Debug.LogError("Subsystem cannot support common ability: " + ability.Key);
                                }
                            }
                        }
                    }
                    abilityScript.Host = shipScript;
                    switch (ability.Key)
                    {
                        case "Attack":
                            shipScript.AttackAbility = (AttackAbilityScript)abilityScript;
                            break;
                        case "Move":
                            shipScript.MoveAbility = (MoveAbilityScript)abilityScript;
                            break;
                        case "Carrier":
                            shipScript.CarrierAbility = (CarrierAbilityScript)abilityScript;
                            break;
                        default:
                            Debug.LogError("Wrong type of common ability: " + ability.Key);
                            break;
                    }
                }

                // Init special ability
                if (libraryData.specialAbilities.Count > 5)
                {
                    Debug.LogWarning($"In {libraryData.unitTypeName}, number of special ability should not excess 5, or UI cannot handle all of them");
                }
                foreach (KeyValuePair<string, List<string>> ability in libraryData.specialAbilities)
                {
                    if (!abilityLibrary.ContainsKey(ability.Key))
                    {
                        Debug.LogError("Cannot find ability: " + ability.Key);
                        continue;
                    }
                    Type abilityType = Type.GetType(abilityLibrary[ability.Key]);
                    SpecialAbilityBaseScript abilityScript = (SpecialAbilityBaseScript)result.AddComponent(abilityType);
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
                                if (temp.GetComponent<SubsystemBaseScript>().supportedSepcialAbility.Contains(
                                    (SpecialAbilityBaseScript.SpecialAbilityType)Enum.Parse(typeof(CommonAbilityBaseScript.CommonAbilityType), ability.Key)))
                                {
                                    abilityScript.SupportedBy.Add(temp.GetComponent<SubsystemBaseScript>());
                                }
                                else
                                {
                                    Debug.LogError("Subsystem cannot support special ability: " + ability.Key);
                                }
                            }
                        }
                    }
                    abilityScript.Host = shipScript;
                    shipScript.SpecialAbilityList.Add(abilityScript);
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
}