using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RTS.UI.CarrierPanel
{
    public class CarrierStatusGridScript : MonoBehaviour
    {
        public Button deployedButton;
        public Text deployedCounter;
        public Button carriedButton;
        public Text carriedCounter;
        public RectTransform deployProgressShadow;
        public RectTransform produceProgressShadow;

        [HideInInspector]
        public string type;

        public void InitStatusGrid(string newType, Sprite icon, CarrierControlPanelScript panel)
        {
            type = newType;
            deployedButton.GetComponent<Image>().sprite = icon;
            carriedButton.GetComponent<Image>().sprite = icon;
            deployedButton.onClick.AddListener(() => panel.SelectUnitsByType(type));
            carriedButton.onClick.AddListener(() => panel.DepolyUnitsByType(type));
        }

        public void UpdateStatusGrid(int deployed, int carried, int capacity, float deployProgress, float produceProgress)
        {
            deployedCounter.text = deployed.ToString() + "/" + capacity.ToString();
            carriedCounter.text = carried.ToString() + "/" + capacity.ToString();
            deployedButton.interactable = deployed != 0;
            carriedButton.interactable = carried != 0;
            deployProgressShadow.offsetMax = new Vector2(deployProgressShadow.offsetMax.x, -80 * Mathf.Clamp01(1 - deployProgress));
            produceProgressShadow.offsetMax = new Vector2(produceProgressShadow.offsetMax.x, -80 * Mathf.Clamp01(1 - produceProgress));
        }
    }
}