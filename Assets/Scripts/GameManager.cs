using Neo.IronLua;
using MLAPI;
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
using RTS.Rendering;
using RTS.Network;
using MLAPI.NetworkVariable;

namespace RTS
{
    public class GameManager : MonoBehaviour
    {
        public enum PlayerRelation
        {
            Self,
            Friend,
            Neutrual,
            Enemy
        }

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
            // not particularly useful, it's just here to show how it could be used
            public Dictionary<string, string> scripts;
        }

        public struct UnitData
        {
            public string type;
            public int belongTo;
            public Dictionary<string, string> specialLuaTags;  // "" -> tag for unit "subsystem anchor name" -> tag for subsystem
            public SerializableVector3 position;
            public SerializableQuaternion rotation;
        }

        public struct GameInitData
        {
            public float mapRadius;
            public List<PlayerData> initPlayerData;

            // Important notes for lua:
            // Global lua is called by game manager with two entry keys: Start and Update (similar to Unity Start and Update)
            // All RTSGO and combined controllable unit have 4 entry keys: OnCreated OnDamaged OnRepaired OnDestroyed
            // Obviously there will be more than those 6 keys in future

            public Dictionary<string, string> initGlobalLua;
            public Dictionary<string, Dictionary<string, string>> initRTSGameObjectLua;

            public List<UnitLibraryData> initUnitLibraryData;
            public List<UnitData> initUnitData;
        }

        private class Player
        {
            public string playerName;
            public float playerMoney;
            public List<int> playerGameObjects = new List<int>();
            public List<int> playerUnits = new List<int>();
        }

        public static GameManager GameManagerInstance { get; private set; }
        public ScriptSystem ScriptSystem { get; private set; }
        public MaterialsManager MaterialsManager { get; private set; }

        [Header("Game Global Setting")] 
        [Tooltip("RTS game object library.")]
        public TextAsset gameObjectLibraryAsset;
        [Tooltip("The time gap between each vision checking.")]
        public float visionProcessGap;

        [Header("Specific Game Rules")]
        [Tooltip("Init game setting.")]
        public TextAsset initDataAsset;

        public int SelfIndex 
        { 
            get 
            { 
                if (LocalPlayerScript.LocalPlayer != null)
                {
                    return LocalPlayerScript.LocalPlayer.PlayerIndex;
                }
                else
                {
                    return -1;
                }
            } 
        }
        public int FrameCount { get; set; } = 0;

        // Read from init data, so we dont need to sycn them
        public float MapRadius { get; private set; } = 0;
        public Dictionary<string, string> GameObjectLibrary { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, UnitLibraryData> UnitLibrary { get; private set; } = new Dictionary<string, UnitLibraryData>();
        private Dictionary<int, Player> allPlayers = new Dictionary<int, Player>();
        private Dictionary<string, string> globalLua = new Dictionary<string, string>();
        private Dictionary<string, Dictionary<string, string>> gameObjectLua = new Dictionary<string, Dictionary<string, string>>();

        // RTSGO tracking sync by calculation
        private int gameObjectIndexCounter = 0;
        private Dictionary<int, GameObject> allGameObjectsDict = new Dictionary<int, GameObject>();
        private List<GameObject> allGameObjectsList = new List<GameObject>();
        private Dictionary<int, GameObject> allUnitsListDict = new Dictionary<int, GameObject>();
        private List<GameObject> allUnitsList = new List<GameObject>();
        // TESTING SOLUTION FOR ENEMY SEARCHING
        public Dictionary<int, List<GameObject>> enemyUnitsTable = new Dictionary<int, List<GameObject>>();

        private float timer = 0;

        void Awake()
        {
            GameManagerInstance = this;
            ScriptSystem = new ScriptSystem();
            MaterialsManager = new MaterialsManager();
            // 添加一个测试材质
            MaterialsManager.Test();
            GameObjectLibrary = JsonConvert.DeserializeObject<Dictionary<string, string>>(gameObjectLibraryAsset.text);

#if UNITY_EDITOR
            NetworkManager.Singleton.StartHost();
#endif

            if (initDataAsset != null)
            {
                GameInitData initData = JsonConvert.DeserializeObject<GameInitData>(initDataAsset.text);
                InitFromInitData(initData);
            }
        }

        private float recordData = 0;
        private float recordData2 = 0;

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (globalLua.TryGetValue("Start", out string code))
                {
                    var script = ScriptSystem.CreateScript("Start", code);
                    ScriptSystem.ExecuteScript(script);
                }
            }
        }

        void FixedUpdate()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                FrameCount++;
                if (globalLua.TryGetValue("Update", out string code))
                {
                    var script = ScriptSystem.CreateScript("Update", code);
                    ScriptSystem.ExecuteScript(script);
                }

                timer += Time.fixedDeltaTime;
                if (timer > visionProcessGap)
                {
                    timer = 0;
                    SetVision();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Debug use
            if (NetworkManager.Singleton.IsServer)
            {
                if (Input.GetKeyDown(KeyCode.O))
                {
                    InstantiateUnit("StandardFrigate", new Vector3(recordData, 0, recordData + recordData2), new Quaternion(), GameObject.Find("RTSGameObject").transform, 0, new Dictionary<string, string>());
                    recordData -= 25;
                    Debug.Log(recordData / 25);
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    InstantiateUnit("StandardFighter", new Vector3(recordData, 500, recordData + recordData2), new Quaternion(), GameObject.Find("RTSGameObject").transform, 0, new Dictionary<string, string>());
                    recordData -= 25;
                    Debug.Log(recordData / 25);
                }
                if (Input.GetKeyDown(KeyCode.I))
                {
                    InstantiateUnit("StandardFrigate", new Vector3(recordData, 0, recordData + recordData2), new Quaternion(), GameObject.Find("RTSGameObject").transform, 1, new Dictionary<string, string>());
                    recordData -= 25;
                    Debug.Log(recordData / 25);
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    recordData = 0;
                    recordData2 -= 25;
                }
            }
        }

        private void SetVision()
        {
            foreach (KeyValuePair<int, Player> i in allPlayers)
            {
                foreach (int iUnit in i.Value.playerUnits)
                {
                    allGameObjectsDict[iUnit].GetComponent<UnitBaseScript>().VisibleTo.Clear();
                    // For every unit of player i, determine with every other players' unit
                    foreach (KeyValuePair<int, Player> j in allPlayers)
                    {
                        if (i.Key == j.Key)
                        {
                            allGameObjectsDict[iUnit].GetComponent<UnitBaseScript>().VisibleTo.Add(i.Key);
                            continue;
                        }
                        foreach (int jUnit in j.Value.playerUnits)
                        {
                            GameObject observer = allGameObjectsDict[jUnit];
                            GameObject observed = allGameObjectsDict[iUnit];
                            if ((observer.transform.position - observed.transform.position).magnitude <=
                                observer.GetComponent<UnitBaseScript>().visionRange)
                            {
                                allGameObjectsDict[iUnit].GetComponent<UnitBaseScript>().VisibleTo.Add(j.Key);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, GameObject> i in allUnitsListDict)
            {
                bool visible = i.Value.GetComponent<UnitBaseScript>().VisibleTo.Contains(SelfIndex);
                foreach (MeshRenderer j in i.Value.GetComponentsInChildren<MeshRenderer>())
                {
                    j.enabled = visible;
                }
            }
        }

        private void InitFromInitData(GameInitData data)
        {
            // Map radius
            MapRadius = data.mapRadius;

            // Player
            foreach (PlayerData i in data.initPlayerData)
            {
                allPlayers.Add(i.index, new Player()
                {
                    playerName = i.name,
                    playerGameObjects = new List<int>()
                });
                enemyUnitsTable[i.index] = new List<GameObject>();
            }

            // Scripts
            globalLua = data.initGlobalLua;
            gameObjectLua = data.initRTSGameObjectLua;

            // UnitLibary
            foreach (UnitLibraryData i in data.initUnitLibraryData)
            {
                UnitLibrary.Add(i.unitTypeName, i);
            }

            if (NetworkManager.Singleton.IsServer)
            {
                // Instantiate units
                foreach (UnitData i in data.initUnitData)
                {
                    if (UnitLibrary.ContainsKey(i.type))
                    {
                        InstantiateUnit(i.type, i.position, i.rotation, GameObject.Find("RTSGameObject").transform, i.belongTo, i.specialLuaTags);
                    }
                    else
                    {
                        Debug.Log("Cannot find unit type" + i.type);
                    }
                }
            }
        }

        public GameObject InstantiateUnit(string unitType, Vector3 position, Quaternion rotation, Transform parent, int belongTo, Dictionary<string, string> luaTags)
        {
            if (!UnitLibrary.ContainsKey(unitType))
            {
                Debug.LogError("No such unit: " + unitType);
                return null;
            }
            UnitLibraryData libraryData = UnitLibrary[unitType];

            // Ship
            GameObject result = Instantiate(Resources.Load<GameObject>(GameObjectLibrary[libraryData.baseTypeName]), position, rotation, parent);

            // 设置一个测试材质
            var resultRenderer = result.AddComponent<RTSGameObjectRenderer>();
            resultRenderer.SetMaterial("test");
            if (UnityEngine.Random.Range(0, 10) >= 5)
            {
                resultRenderer.SetProperties(p => p.SetColor("_Color", Color.green));
            }

            if (result.GetComponent<UnitBaseScript>() != null)
            {
                UnitBaseScript unitScript = result.GetComponent<UnitBaseScript>();
                unitScript.UnitTypeID = unitType;
                unitScript.PropertyDictionary = libraryData.properties;

                // Apply lua tag
                if (luaTags != null && luaTags.TryGetValue("", out string unitTag))
                {
                    unitScript.LuaTag = unitTag;
                }

                // Init subsystem
                foreach (UnitBaseScript.AnchorData anchorData in unitScript.subsyetemAnchors)
                {
                    // Already have a subsystem (set in prefab)
                    if (anchorData.subsystem != null)
                    {
                        anchorData.subsystem.GetComponent<SubsystemBaseScript>().Host = unitScript;
                        // Apply lua tag
                        if (luaTags != null && luaTags.TryGetValue(anchorData.anchorName, out string subsystemTag))
                        {
                            anchorData.subsystem.GetComponent<SubsystemBaseScript>().LuaTag = subsystemTag;
                        }
                    }
                    else if (libraryData.subsystems.ContainsKey(anchorData.anchorName) && anchorData.subsystem == null)
                    {
                        string subsystemTypeName = libraryData.subsystems[anchorData.anchorName];
                        GameObject temp = Instantiate(Resources.Load<GameObject>(GameObjectLibrary[subsystemTypeName]), anchorData.anchor.transform);
                        SubsystemBaseScript subsystemScript = temp.GetComponent<SubsystemBaseScript>();
                        if (anchorData.subsystemScale != subsystemScript.type)
                        {
                            Debug.Log("Subsystem mismatch with anchor type: " + anchorData.subsystemScale + " and " + subsystemScript.type);
                        }
                        else
                        {
                            anchorData.subsystem = temp;
                            subsystemScript.Host = unitScript;
                        }
                        // Apply lua tag
                        if (luaTags != null && luaTags.TryGetValue(anchorData.anchorName, out string subsystemTag))
                        {
                            subsystemScript.LuaTag = subsystemTag;
                        }
                    }
                }

                // Init common ability
                foreach (KeyValuePair<string, List<string>> ability in libraryData.commonAbilities)
                {
                    Type abilityType;
                    switch (ability.Key)
                    {
                        case "Attack":
                            abilityType = Type.GetType("RTS.Ability.CommonAbility.AttackAbilityScript");
                            break;
                        case "Move":
                            abilityType = Type.GetType("RTS.Ability.CommonAbility.MoveAbilityScript");
                            break;
                        case "Carrier":
                            abilityType = Type.GetType("RTS.Ability.CommonAbility.CarrierAbilityScript");
                            break;
                        default:
                            Debug.LogError("Wrong type of common ability: " + ability.Key);
                            continue;
                    }
                    CommonAbilityBaseScript abilityScript = (CommonAbilityBaseScript)result.AddComponent(abilityType);
                    foreach (string supportedSubsystemAnchor in ability.Value)
                    {
                        if (supportedSubsystemAnchor != libraryData.baseTypeName)
                        {
                            GameObject temp = unitScript.subsyetemAnchors.FirstOrDefault(x => x.anchorName == supportedSubsystemAnchor).subsystem;
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
                    abilityScript.Host = unitScript;
                    switch (ability.Key)
                    {
                        case "Attack":
                            unitScript.AttackAbility = (AttackAbilityScript)abilityScript;
                            break;
                        case "Move":
                            unitScript.MoveAbility = (MoveAbilityScript)abilityScript;
                            break;
                        case "Carrier":
                            unitScript.CarrierAbility = (CarrierAbilityScript)abilityScript;
                            break;
                        default:
                            Debug.LogError("Wrong type of common ability: " + ability.Key);
                            break;
                    }
                }

                foreach (SpecialAbilityBaseScript speaiclAbility in result.transform.GetComponentsInChildren<SpecialAbilityBaseScript>())
                {
                    speaiclAbility.Host = unitScript;
                    if (unitScript.SpecialAbilityList.ContainsKey(speaiclAbility.specialAbilityID))
                    {
                        unitScript.SpecialAbilityList[speaiclAbility.specialAbilityID].Add(speaiclAbility);
                    }
                    else
                    {
                        unitScript.SpecialAbilityList.Add(speaiclAbility.specialAbilityID, new List<SpecialAbilityBaseScript>() { speaiclAbility });
                    }
                }
            }

            // Set belonging and index
            result.GetComponent<RTSGameObjectBaseScript>().BelongTo = belongTo;
            foreach (RTSGameObjectBaseScript i in result.GetComponentsInChildren<RTSGameObjectBaseScript>())
            {
                i.BelongTo = belongTo;
                i.Index = gameObjectIndexCounter;
                gameObjectIndexCounter++;
            }
            return result;
        }

        public void InstantiateUnitClient(GameObject result)
        {
            UnitBaseScript unitScript = result.GetComponent<UnitBaseScript>();
            if (unitScript == null)
            {
                return;
            }
            UnitLibraryData libraryData = UnitLibrary[unitScript.UnitTypeID];
            // Init common ability
            foreach (KeyValuePair<string, List<string>> ability in libraryData.commonAbilities)
            {
                Type abilityType;
                switch (ability.Key)
                {
                    case "Attack":
                        abilityType = Type.GetType("RTS.Ability.CommonAbility.AttackAbilityScript");
                        break;
                    case "Move":
                        abilityType = Type.GetType("RTS.Ability.CommonAbility.MoveAbilityScript");
                        break;
                    case "Carrier":
                        abilityType = Type.GetType("RTS.Ability.CommonAbility.CarrierAbilityScript");
                        break;
                    default:
                        Debug.LogError("Wrong type of common ability: " + ability.Key);
                        continue;
                }
                CommonAbilityBaseScript abilityScript = (CommonAbilityBaseScript)result.AddComponent(abilityType);
                foreach (string supportedSubsystemAnchor in ability.Value)
                {
                    if (supportedSubsystemAnchor != libraryData.baseTypeName)
                    {
                        GameObject temp = unitScript.subsyetemAnchors.FirstOrDefault(x => x.anchorName == supportedSubsystemAnchor).subsystem;
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
                abilityScript.Host = unitScript;
                switch (ability.Key)
                {
                    case "Attack":
                        unitScript.AttackAbility = (AttackAbilityScript)abilityScript;
                        break;
                    case "Move":
                        unitScript.MoveAbility = (MoveAbilityScript)abilityScript;
                        break;
                    case "Carrier":
                        unitScript.CarrierAbility = (CarrierAbilityScript)abilityScript;
                        break;
                    default:
                        Debug.LogError("Wrong type of common ability: " + ability.Key);
                        break;
                }
            }

            foreach (SpecialAbilityBaseScript speaiclAbility in result.transform.GetComponentsInChildren<SpecialAbilityBaseScript>())
            {
                speaiclAbility.Host = unitScript;
                if (unitScript.SpecialAbilityList.ContainsKey(speaiclAbility.specialAbilityID))
                {
                    unitScript.SpecialAbilityList[speaiclAbility.specialAbilityID].Add(speaiclAbility);
                }
                else
                {
                    unitScript.SpecialAbilityList.Add(speaiclAbility.specialAbilityID, new List<SpecialAbilityBaseScript>() { speaiclAbility });
                }
            }
        }

        public List<GameObject> GetGameObjectForPlayer(int index)
        {
            List<GameObject> result = new List<GameObject>();
            if (allPlayers.ContainsKey(index))
            {
                foreach (int i in allPlayers[index].playerGameObjects)
                {
                    result.Add(allGameObjectsDict[i]);
                }
            }
            return result;
        }

        public PlayerRelation GetPlayerRelation(int playerIndex1, int playerIndex2)
        {
            // TODO: Extend enemy to friend neutrual enemy
            if (playerIndex1 == playerIndex2)
            {
                return PlayerRelation.Self;
            }
            else
            {
                return PlayerRelation.Enemy;
            }
        }

        public void OnGameObjectCreated(GameObject self)
        {
            // Lua
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                LuaTable selfTable = ScriptSystem.SetRTSGameObjectInfo(self.GetComponent<RTSGameObjectBaseScript>());
                if (gameObjectLua.TryGetValue(self.GetComponent<RTSGameObjectBaseScript>().typeID, out Dictionary<string, string> matchedLua))
                {
                    if (matchedLua.TryGetValue("OnCreated", out string code))
                    {
                        var script = ScriptSystem.CreateScript("OnCreated", code, new KeyValuePair<string, Type>("self", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable);
                    }
                }
                if (self.GetComponent<UnitBaseScript>() != null)
                {
                    UnitLibraryData libraryData = UnitLibrary[self.GetComponent<UnitBaseScript>().UnitTypeID];
                    if (libraryData.scripts != null && libraryData.scripts.TryGetValue("OnCreated", out var code))
                    {
                        var script = ScriptSystem.CreateScript("OnCreated", code, new KeyValuePair<string, Type>("self", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable);
                    }
                }
            }

            // Index
            int gameObjectIndex = self.GetComponent<RTSGameObjectBaseScript>().Index;
            allGameObjectsDict.Add(gameObjectIndex, self);
            allGameObjectsList.Add(self);

            // Player
            int playerIndex = self.GetComponent<RTSGameObjectBaseScript>().BelongTo;
            allPlayers[playerIndex].playerGameObjects.Add(gameObjectIndex);
            if (self.GetComponent<UnitBaseScript>() != null)
            {
                allPlayers[playerIndex].playerUnits.Add(gameObjectIndex);
                allUnitsListDict.Add(gameObjectIndex, self);
                allUnitsList.Add(self);
                // TODO: relation check
                foreach (KeyValuePair<int, Player> i in allPlayers)
                {
                    if (i.Key != playerIndex)
                    {
                        enemyUnitsTable[i.Key].Add(self);
                    }
                }
            }

        }

        public void OnGameObjectDamaged(GameObject self, GameObject other)
        {
            // Lua
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                LuaTable selfTable = ScriptSystem.SetRTSGameObjectInfo(self.GetComponent<RTSGameObjectBaseScript>());
                LuaTable otherTable = ScriptSystem.SetRTSGameObjectInfo(other != null ? other.GetComponent<RTSGameObjectBaseScript>() : null);
                if (gameObjectLua.TryGetValue(self.GetComponent<RTSGameObjectBaseScript>().typeID, out Dictionary<string, string> matchedLua))
                {
                    if (matchedLua.TryGetValue("OnDamaged", out string code))
                    {
                        var script = ScriptSystem.CreateScript("OnDamaged", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
                if (self.GetComponent<UnitBaseScript>() != null)
                {
                    UnitLibraryData libraryData = UnitLibrary[self.GetComponent<UnitBaseScript>().UnitTypeID];
                    if (libraryData.scripts != null && libraryData.scripts.TryGetValue("OnDamaged", out var code))
                    {
                        var script = ScriptSystem.CreateScript("OnDamaged", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
            }
        }

        public void OnGameObjectRepaired(GameObject self, GameObject other)
        {
            // Lua
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                LuaTable selfTable = ScriptSystem.SetRTSGameObjectInfo(self.GetComponent<RTSGameObjectBaseScript>());
                LuaTable otherTable = ScriptSystem.SetRTSGameObjectInfo(other != null ? other.GetComponent<RTSGameObjectBaseScript>() : null);
                if (gameObjectLua.TryGetValue(self.GetComponent<RTSGameObjectBaseScript>().typeID, out Dictionary<string, string> matchedLua))
                {
                    if (matchedLua.TryGetValue("OnRepaired", out string code))
                    {
                        var script = ScriptSystem.CreateScript("OnRepaired", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
                if (self.GetComponent<UnitBaseScript>() != null)
                {
                    UnitLibraryData libraryData = UnitLibrary[self.GetComponent<UnitBaseScript>().UnitTypeID];
                    if (libraryData.scripts != null && libraryData.scripts.TryGetValue("OnRepaired", out var code))
                    {
                        var script = ScriptSystem.CreateScript("OnRepaired", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
            }
        }

        public void OnGameObjectDestroyed(GameObject self, GameObject other)
        {
            // Lua
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                LuaTable selfTable = ScriptSystem.SetRTSGameObjectInfo(self.GetComponent<RTSGameObjectBaseScript>());
                LuaTable otherTable = ScriptSystem.SetRTSGameObjectInfo(other != null ? other.GetComponent<RTSGameObjectBaseScript>() : null);
                if (gameObjectLua.TryGetValue(self.GetComponent<RTSGameObjectBaseScript>().typeID, out Dictionary<string, string> matchedLua))
                {
                    if (matchedLua.TryGetValue("OnDestroyed", out string code))
                    {
                        var script = ScriptSystem.CreateScript("OnDestroyed", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
                if (self.GetComponent<UnitBaseScript>() != null)
                {
                    UnitLibraryData libraryData = UnitLibrary[self.GetComponent<UnitBaseScript>().UnitTypeID];
                    if (libraryData.scripts != null && libraryData.scripts.TryGetValue("OnDestroyed", out var code))
                    {
                        var script = ScriptSystem.CreateScript("OnDestroyed", code,
                            new KeyValuePair<string, Type>("self", typeof(LuaTable)), new KeyValuePair<string, Type>("other", typeof(LuaTable)));
                        ScriptSystem.ExecuteScript(script, selfTable, otherTable);
                    }
                }
            }
           
            // Index
            int gameObjectIndex = self.GetComponent<RTSGameObjectBaseScript>().Index;
            allGameObjectsDict.Remove(gameObjectIndex);
            allGameObjectsList.Remove(self);

            // Player
            int playerIndex = self.GetComponent<RTSGameObjectBaseScript>().BelongTo;
            allPlayers[playerIndex].playerGameObjects.Remove(gameObjectIndex);
            if (self.GetComponent<UnitBaseScript>() != null)
            {
                allPlayers[playerIndex].playerUnits.Remove(gameObjectIndex);
                allUnitsListDict.Remove(gameObjectIndex);
                allUnitsList.Remove(self);
                foreach (KeyValuePair<int, Player> i in allPlayers)
                {
                    enemyUnitsTable[i.Key].Remove(self);
                }
            }
        }

        public GameObject GetGameObjectByIndex(int index)
        {
            if (allGameObjectsDict.ContainsKey(index))
            {
                return allGameObjectsDict[index];
            }
            return null;
        }

        public UnitBaseScript GetUnitByIndex(int index)
        {
            if (allUnitsListDict.ContainsKey(index))
            {
                return allGameObjectsDict[index].GetComponent<UnitBaseScript>();
            }
            return null;
        }


        public ref List<GameObject> GetAllGameObjects()
        {
            return ref allGameObjectsList;
        }

        public ref List<GameObject> GetAllUnits()
        {
            return ref allUnitsList;
        }
    }
}