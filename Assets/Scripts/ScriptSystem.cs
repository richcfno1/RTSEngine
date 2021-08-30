using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.IronLua;
using System;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;

namespace RTS
{
    public class ScriptSystem
    {
        public class Script
        {
            public string name;
            public LuaChunk script;
        }

        private GameManager gameManager;
        private GameObject masterGameObject;
        private Dictionary<Type, Func<LuaTable, string, object>> luaToCSConverters;
        private Dictionary<Type, Func<object, LuaTable>> cSToLuaConverters;
        private Lua lua;
        private LuaCompileOptions compileOptions;
        private LuaGlobal env;

        public ScriptSystem()
        {
            gameManager = GameManager.GameManagerInstance;
            masterGameObject = GameObject.Find("RTSGameObject");
            luaToCSConverters = new Dictionary<Type, Func<LuaTable, string, object>>();
            cSToLuaConverters = new Dictionary<Type, Func<object, LuaTable>>();
            AddLuaToCSConverter<Vector3>(GetVector3);
            AddCSToLuaConverter<Vector3>(SetVector3);
            AddLuaToCSConverter<Quaternion>(GetQuaternion);
            AddCSToLuaConverter<Quaternion>(SetQuaternion);
            lua = new Lua(LuaIntegerType.Int32, LuaFloatType.Float);
            compileOptions = new LuaCompileOptions { /* ... */};
            env = lua.CreateEnvironment();

            Register("Time", _ => Time.timeSinceLevelLoad);
            Register("LogicalFrame", _ => GameManager.GameManagerInstance.FrameCount);

            Register(nameof(Test), Test);
            Register(nameof(HelloWorld), HelloWorld);

            Register(nameof(TestGetTable), TestGetTable);
            Register(nameof(LogText), LogText);
        }

        public Script CreateScript(string description, string code)
        {
            return new Script
            {
                name = description,
                script = lua.CompileChunk(code, description, compileOptions)
            };
        }

        public void ExecuteScript(Script script)
        {
            script.script.Run(env);
        }

        public void SetRTSGameObjectInfo(string name, RTSGameObjectBaseScript gameObject)
        {
            LuaTable temp = new LuaTable();
            temp.Add("index", gameObject.Index);
            temp.Add("belongTo", gameObject.BelongTo);
            temp.Add("type", gameObject.typeID);
            if (gameObject.GetComponent<UnitBaseScript>() != null)
            {
                temp.Add("unitType", gameObject.GetComponent<UnitBaseScript>().UnitTypeID);
            }
            temp.Add("position", gameObject.transform.position);
            temp.Add("rotation", gameObject.transform.rotation);
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                temp.Add("velocity", gameObject.GetComponent<Rigidbody>().velocity);
            }
            env[name] = temp;
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
            if (typeof(T).IsArray)
            {
                // TODO
            }
            if (luaToCSConverters.TryGetValue(typeof(T), out var converter))
            {
                return (T)converter(table, name);
            }
            return (T)Convert.ChangeType(table[name], typeof(T));
        }

        void AddLuaToCSConverter<T>(Func<LuaTable, string, T> converter)
        {
            luaToCSConverters.Add(typeof(T), (t, k) => (object)converter(t, k));
        }

        void AddCSToLuaConverter<T>(Func<T, LuaTable> converter)
        {
            cSToLuaConverters.Add(typeof(T), t => converter((T)t));
        }

        // Data types:
        Vector3 GetVector3(LuaTable table, string name)
        {
            var vector = GetArgument<LuaTable>(table, name);
            var x = GetArgument<float>(vector, "x");
            var y = GetArgument<float>(vector, "y");
            var z = GetArgument<float>(vector, "z");
            return new Vector3(x, y, z);
        }

        LuaTable SetVector3(Vector3 value)
        {
            LuaTable temp = new LuaTable();
            temp.Add("x", value.x);
            temp.Add("y", value.y);
            temp.Add("z", value.z);
            return temp;
        }

        Quaternion GetQuaternion(LuaTable table, string name)
        {
            var quaternion = GetArgument<LuaTable>(table, name);
            var x = GetArgument<float>(quaternion, "x");
            var y = GetArgument<float>(quaternion, "y");
            var z = GetArgument<float>(quaternion, "z");
            var w = GetArgument<float>(quaternion, "w");
            return new Quaternion(x, y, z, w);
        }

        LuaTable SetQuaternion(Quaternion value)
        {
            LuaTable temp = new LuaTable();
            temp.Add("x", value.x);
            temp.Add("y", value.y);
            temp.Add("z", value.z);
            temp.Add("w", value.w);
            return temp;
        }

        // Debug lua functions
        void Test(LuaTable arguments)
        {
            var unitType = GetArgument<string>(arguments, "unitType");
            var position = GetArgument<Vector3>(arguments, "position");
            var belongsTo = GetArgument<int>(arguments, "belongTo");
            gameManager.InstantiateUnit(unitType, position, Quaternion.identity, masterGameObject.transform, belongsTo);
        }

        void HelloWorld(LuaTable arguments)
        {
            Debug.Log("Hello world!");
        }

        // Default lua functions
        LuaTable TestGetTable (LuaTable arguments)
        {
            LuaTable result = new LuaTable();
            result.Add("intValue", 123321);
            result.Add("stringValue", "abc");
            result.Add("vector3Value", cSToLuaConverters[typeof(Vector3)](Vector3.one));
            return result;
        }

        void LogText(LuaTable arguments)
        {
            string text = GetArgument<string>(arguments, "text");
            Debug.Log(text);
        }
    }
}