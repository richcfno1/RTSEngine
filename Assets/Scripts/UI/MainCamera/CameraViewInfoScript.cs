using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI.MainCamera
{
    public class CameraViewInfoScript : MonoBehaviour
    {
        public Slider hpdata;

        public void ChangeColor(Color color)
        {
            foreach (Image i in GetComponentsInChildren<Image>())
            {
                i.color = color;
            }
        }
    }
}
