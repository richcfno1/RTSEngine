using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.UI.Command;

namespace RTS.Game.UI.GroupPanel
{
    public class GroupPanelScript : MonoBehaviour
    {
        public List<GroupSingleIconScript> allGrids;

        // Update is called once per frame
        void Update()
        {
            foreach (GroupSingleIconScript i in allGrids)
            {
                if (i.index < SelectControlScript.SelectionControlInstance.UnitGroup.Count &&
                    SelectControlScript.SelectionControlInstance.UnitGroup[i.index].Count > 0)
                {
                    i.gameObject.SetActive(true);
                }
                else
                {
                    i.gameObject.SetActive(false);
                }
            }
        }
    }
}


