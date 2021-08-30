using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.IronLua;
using System;

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
        private Dictionary<Type, Func<LuaTable, string, object>> converters;
        private Lua lua;
        private LuaCompileOptions compileOptions;
        private LuaGlobal env;

        public ScriptSystem()
        {
            gameManager = GameManager.GameManagerInstance;
            masterGameObject = GameObject.Find("RTSGameObject");
            converters = new Dictionary<Type, Func<LuaTable, string, object>>();
            AddConverter(GetVector3);
            lua = new Lua(LuaIntegerType.Int32, LuaFloatType.Float);
            compileOptions = new LuaCompileOptions { /* ... */};
            env = lua.CreateEnvironment();
            Register("Time", _ => Time.timeSinceLevelLoad);
            Register("LogicalFrame", _ => GameManager.GameManagerInstance.FrameCount);
            Register("TestPassTable", _ =>
            {
                LuaTable temp = new LuaTable();
                temp.Add("TestKey1", "TestValue1");
                return temp;
            });
            Register(nameof(Test), Test);
            Register(nameof(HelloWorld), HelloWorld);
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
            if (converters.TryGetValue(typeof(T), out var converter))
            {
                return (T)converter(table, name);
            }
            return (T)Convert.ChangeType(table[name], typeof(T));
        }

        void AddConverter<T>(Func<LuaTable, string, T> converter)
        {
            converters.Add(typeof(T), (t, k) => (object)converter(t, k));
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

        // Debug lua functions
        void Test(LuaTable arguments)
        {
            var unitType = GetArgument<string>(arguments, "unitType");
            var position = GetArgument<Vector3>(arguments, "position");
            var belongsTo = GetArgument<int>(arguments, "belongsTo");
            gameManager.InstantiateUnit(unitType, position, Quaternion.identity, masterGameObject.transform, belongsTo);
        }

        void HelloWorld(LuaTable arguments)
        {
            Debug.Log("Hello world!");
        }

        // Default lua functions
        void LogText(LuaTable arguments)
        {
            string text = GetArgument<string>(arguments, "text");
            Debug.Log(text);
        }
    }
}