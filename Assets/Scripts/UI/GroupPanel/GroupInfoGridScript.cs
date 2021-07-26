using System.Linq ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.UI.Command;
using RTS.RTSGameObject;

namespace RTS.UI.GroupPanel
{
    public class GroupInfoGridScript : MonoBehaviour
    {
        public int index;
        public Text amount;
        public Image icon;

        // Update is called once per frame
        void Update()
        {
            List<GameObject> thisGroup = SelectControlScript.SelectionControlInstance.UnitGroup[index];
            amount.text = $"{thisGroup.Count}";
            if (thisGroup.Count != 0)
            {
                icon.sprite = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                    gameObjectLibrary[thisGroup[0].GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
            }
        }

        public void SelectGroup()
        {
            SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(
                SelectControlScript.SelectionControlInstance.UnitGroup[index]);
        }
    }
}

