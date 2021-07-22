using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject
{
    [CreateAssetMenu(fileName = "RTSGameObjectData", menuName = "RTSEngine/Create RTSGameObjectData", order = 1)]
    public class RTSGameObjectData : ScriptableObject
    {
        public GameObject prefab;
        public Sprite icon;
    }
}