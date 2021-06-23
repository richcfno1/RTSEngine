using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;

namespace RTS.UI.SelectedPanel
{
    public class SelectedDetailGridScript : MonoBehaviour
    {
        public Image icon;
        public Text typeName;
        public Slider hpdata;
        public Image AttackUp;
        public Image AttackDown;
        public Image DefenceUp;
        public Image DefenceDown;
        public Image MoveUp;
        public Image MoveDown;
        public Text otherInfo;

        public void UpdateDetailGrid(GameObject targetRTSGameObject)
        {
            hpdata.value = targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().HP / targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().maxHP;
            UnitBaseScript tempUnitScript = targetRTSGameObject.GetComponent<UnitBaseScript>();
            if (tempUnitScript != null)
            {
                typeName.text = tempUnitScript.UnitTypeID;
                // Attack
                if (tempUnitScript.AttackPower > 1)
                {
                    AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.AttackPower - 1 + 0.5f));
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.AttackPower < 1)
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.AttackPower + 0.5f));
                }
                else
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                // Defence
                if (tempUnitScript.DefencePower > 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.DefencePower - 1 + 0.5f));
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.DefencePower < 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.DefencePower + 0.5f));
                }
                else
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                // Move
                if (tempUnitScript.MovePower > 1)
                {
                    MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.MovePower - 1 + 0.5f));
                    MoveDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.MovePower < 1)
                {
                    MoveUp.color = new Color(1, 1, 1, 0);
                    MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.MovePower + 0.5f));
                }
                else
                {
                    MoveUp.color = new Color(1, 1, 1, 0);
                    MoveDown.color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                typeName.text = targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().typeID;
                AttackUp.color = new Color(1, 1, 1, 0);
                AttackDown.color = new Color(1, 1, 1, 0);
                DefenceUp.color = new Color(1, 1, 1, 0);
                DefenceDown.color = new Color(1, 1, 1, 0);
                MoveUp.color = new Color(1, 1, 1, 0);
                MoveDown.color = new Color(1, 1, 1, 0);
            }
        }
    }
}
