using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Rendering
{
    public class MaterialsManager
    {
        private readonly Dictionary<string, SerializableMaterial> materials;
        private readonly Dictionary<string, Action<MaterialPropertyBlock>> setters;
        private readonly Dictionary<string, Material> instances;

        public MaterialsManager()
        {
            materials = new Dictionary<string, SerializableMaterial>();
            setters = new Dictionary<string, Action<MaterialPropertyBlock>>();
            instances = new Dictionary<string, Material>();
        }

        public void Test()
        {
            RegisterMaterial("test", new SerializableMaterial
            {
                shader = "Custom/RTSDefault",
                values = new Dictionary<string, Value>
                {
                    ["_Color"] = new Value { type = Value.Type.Color, value = Color.red }
                }
            });
        }

        public void Reset()
        {
            foreach(var material in instances.Values)
            {
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                }
            }
            instances.Clear();
            setters.Clear();
            materials.Clear();
        }

        public void RegisterMaterial(string key, SerializableMaterial material)
        {
            materials.Add(key, material);
            setters.Add(key, material.CreateSetter(this));
            if (!instances.ContainsKey(key))
            {
                instances.Add(key, new Material(Shader.Find(material.shader)));
            }
        }

        public Material GetMaterial(string id)
        {
            return instances[id];
        }

        public Action<MaterialPropertyBlock> GetPropertySetter(string id)
        {
            return setters[id];
        }

        public Texture RetrieveTexture(string id)
        {
            throw new NotImplementedException();
        }
    }
}