using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.IronLua;
using System;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using RTS.UI;

namespace RTS
{
    public class ScriptSystem
    {
        public class Script
        {
            public string name;
            public LuaChunk script;
        }

        private readonly GameManager gameManager;
        private readonly GameObject masterGameObject;
        private readonly Lua lua;
        private readonly LuaCompileOptions compileOptions;
        private readonly LuaGlobal env;

        public ScriptSystem()
        {
            gameManager = GameManager.GameManagerInstance;
            masterGameObject = GameObject.Find("RTSGameObject");
            lua = new Lua(LuaIntegerType.Int32, LuaFloatType.Float);
            compileOptions = new LuaCompileOptions { /* ... */};
            env = lua.CreateEnvironment();

            Register("Time", _ => Time.timeSinceLevelLoad);
            Register("LogicalFrame", _ => GameManager.GameManagerInstance.FrameCount);

            Register(nameof(HelloWorld), HelloWorld);
            Register(nameof(InitSFFromLuaTest), InitSFFromLuaTest);

            Register(nameof(DebugText), DebugText);
            Register(nameof(LogText), LogText);
        }

        public Script CreateScript(string description, string code, params KeyValuePair<string, Type>[] arguments)
        {
            return new Script
            {
                name = description,
                script = lua.CompileChunk(code, description, compileOptions, arguments)
            };
        }

        public void ExecuteScript(Script script, params object[] arguments)
        {
            env.DoChunk(script.script, arguments);
        }

        public LuaTable SetRTSGameObjectInfo(RTSGameObjectBaseScript gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }
            LuaTable temp = new LuaTable();
            temp.Add("index", gameObject.Index);
            temp.Add("belongTo", gameObject.BelongTo);
            temp.Add("luaTag", gameObject.LuaTag);
            temp.Add("type", gameObject.typeID);
            temp.Add("position", gameObject.transform.position);
            temp.Add("rotation", gameObject.transform.rotation);
            if (gameObject.GetComponent<UnitBaseScript>() != null)
            {
                temp.Add("unitType", gameObject.GetComponent<UnitBaseScript>().UnitTypeID);
            }
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                temp.Add("velocity", gameObject.GetComponent<Rigidbody>().velocity);
            }
            return temp;
        }

        // Function register
        void Register<T>(string name, Func<LuaTable, T> function)
        {
            env[name] = function;
        }

        void Register(string name, Action<LuaTable> action)
        {
            env[name] = action;
        }

        // Data type converter
        T GetArgument<T>(LuaTable table, string name)
        {
            return (T)Convert.ChangeType(table[name], typeof(T));
        }

        // Debug lua functions
        void HelloWorld(LuaTable arguments)
        {
            Debug.Log("Hello world!");
        }

        void InitSFFromLuaTest(LuaTable arguments)
        {
            var unitType = GetArgument<string>(arguments, "unitType");
            var position = GetArgument<Vector3>(arguments, "position");
            var belongsTo = GetArgument<int>(arguments, "belongTo");
            gameManager.InstantiateUnit(unitType, position, Quaternion.identity, masterGameObject.transform, belongsTo, new Dictionary<string, string>());
        }


        // Default lua functions
        void DebugText(LuaTable arguments)
        {
            string text = GetArgument<string>(arguments, "text");
            Debug.Log(text);
        }

        void LogText(LuaTable arguments)
        {
            string text = GetArgument<string>(arguments, "text");
            float displayTime = GetArgument<float>(arguments, "displayTime");
            LogPanelScript.LogPanelScriptInstance.DisplayText(text, displayTime);
        }
    }
}