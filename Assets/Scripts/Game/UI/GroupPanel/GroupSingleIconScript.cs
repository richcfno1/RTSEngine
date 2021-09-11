using System.Linq ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.Game.UI.Command;
using RTS.Game.RTSGameObject;

namespace RTS.Game.UI.GroupPanel
{
    public class GroupSingleIconScript : MonoBehaviour
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
                if (thisGroup[0] != null)
                {
                    icon.sprite = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                        GameObjectLibrary[thisGroup[0].GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                }
            }
        }

        public void SelectGroup()
        {
            SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(
                SelectControlScript.SelectionControlInstance.UnitGroup[index]);
        }
    }
}

