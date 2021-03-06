using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.Game.RTSGameObject;
using RTS.Game.RTSGameObject.Unit;

namespace RTS.Game.UI.SelectedPanel
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
                if (tempUnitScript.AttackPowerRatio > 1)
                {
                    AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.AttackPowerRatio - 1 + 0.5f));
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.AttackPowerRatio < 1)
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.AttackPowerRatio + 0.5f));
                }
                else
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                // Defence
                if (tempUnitScript.DefencePowerRatio > 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.DefencePowerRatio - 1 + 0.5f));
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.DefencePowerRatio < 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.DefencePowerRatio + 0.5f));
                }
                else
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                // Move
                if (tempUnitScript.MovePowerRatio > 1)
                {
                    MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.MovePowerRatio - 1 + 0.5f));
                    MoveDown.color = new Color(1, 1, 1, 0);
                }
                else if (tempUnitScript.MovePowerRatio < 1)
                {
                    MoveUp.color = new Color(1, 1, 1, 0);
                    MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.MovePowerRatio + 0.5f));
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
