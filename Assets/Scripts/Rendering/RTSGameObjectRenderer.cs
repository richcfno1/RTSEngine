using System;
using System.Collections;
using UnityEngine;

namespace RTS.Rendering
{
    public class RTSGameObjectRenderer : MonoBehaviour
    {
        private static MaterialsManager Materials => GameManager.GameManagerInstance.MaterialsManager;

        private new Renderer renderer;
        private MaterialPropertyBlock properties;

        // Use this for initialization
        void Awake()
        {
            renderer = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
        }

        public void SetMaterial(string id)
        {
            renderer.sharedMaterial = Materials.GetMaterial(id);
            renderer.GetPropertyBlock(properties);
            var setter = Materials.GetPropertySetter(id);
            setter(properties);
            renderer.SetPropertyBlock(properties);
        }

        public void SetProperties(Action<MaterialPropertyBlock> setter)
        {
            renderer.GetPropertyBlock(properties);
            setter(properties);
            renderer.SetPropertyBlock(properties);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}