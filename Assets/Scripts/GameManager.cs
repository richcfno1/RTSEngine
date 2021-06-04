using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.IronLua;
using System;

public class GameManager : MonoBehaviour
{
    public struct PlayerData
    {
        public int index;
        public string name;
    }

    public struct GameObjectData
    {
        public int index;
        public string typeName;
        public int belongTo;
        Vector3 position;
    }

    public struct GameInitData
    {
        public List<PlayerData> initAllPlayers;
        public List<GameObject> initAllGameObjects;
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
    Lua gameLua = new Lua();

    void Awake()
    {
        GameManagerInstance = this; 
        InitFromInitData(new GameInitData()
        {
            initAllPlayers = new List<PlayerData>()
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
        foreach (PlayerData i in data.initAllPlayers)
        {
            allPlayers.Add(i.index, new Player()
            {
                playerName = i.name,
                playerGameObjects = new List<int>()
            });
        }
        // TODO: init gameobjects
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
