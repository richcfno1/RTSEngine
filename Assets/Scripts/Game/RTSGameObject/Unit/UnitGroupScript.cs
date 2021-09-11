using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Game.RTSGameObject.Unit
{
    public class UnitGroupScript : MonoBehaviour
    {
        [Serializable]
        public class UnitGroupData
        {
            public List<Vector3> relativePositions;
        }

        public string GroupTag { get; set; }
        public List<Vector3> RelativePositions { get; set; }
        public List<GameObject> GroupedUnits { get; set; }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}