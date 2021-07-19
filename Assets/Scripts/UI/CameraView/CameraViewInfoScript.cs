using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.RTSGameObject;

namespace RTS.UI.CameraView
{
    public class CameraViewInfoScript : MonoBehaviour
    {
        public Slider hpdata;
        [HideInInspector]
        public RTSGameObjectBaseScript bindObject;

        public void ChangeColor(Color color)
        {
            foreach (Image i in GetComponentsInChildren<Image>())
            {
                i.color = color;
            }
        }
    }
}
