using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.Game.RTSGameObject;
using RTS.Game.RTSGameObject.Unit;

namespace RTS.Game.UI.SelectedPanel
{
    public class SelectedInfoGridScript : MonoBehaviour
    {
        public float normalAlphaValue;
        public float highlightAlphaValue;
        public Image icon;
        public Slider hpdata;
        public Text numberCount;
        public Image AttackUp;
        public Image AttackDown;
        public Image DefenceUp;
        public Image DefenceDown;
        public Image MoveUp;
        public Image MoveDown;

        public bool IsMainSelectedGrid { get; private set; } = false;

        public void UpdateInfoGrid(GameObject targetRTSGameObject)
        {
            hpdata.value = targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().HP / targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().maxHP;
            UnitBaseScript tempUnitScript = targetRTSGameObject.GetComponent<UnitBaseScript>();
            if (tempUnitScript != null)
            {
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
                AttackUp.color = new Color(1, 1, 1, 0);
                AttackDown.color = new Color(1, 1, 1, 0);
                DefenceUp.color = new Color(1, 1, 1, 0);
                DefenceDown.color = new Color(1, 1, 1, 0);
                MoveUp.color = new Color(1, 1, 1, 0);
                MoveDown.color = new Color(1, 1, 1, 0);
            }
            numberCount.text = "";
        }

        public void UpdateInfoGrid(List<GameObject> allGameObjects)
        {
            bool isUnit = true;
            int count = 0;
            float avgHP = 0;
            float maxHP = 0;
            float avgAttackPower = 0;
            float avgDefencePower = 0;
            float avgMovePower = 0;
            foreach (GameObject i in allGameObjects)
            {
                if (i != null)
                {
                    if (i.GetComponent<UnitBaseScript>() != null)
                    {
                        count++;
                        avgHP += i.GetComponent<UnitBaseScript>().HP;
                        maxHP = i.GetComponent<UnitBaseScript>().maxHP;
                        avgAttackPower += i.GetComponent<UnitBaseScript>().AttackPowerRatio;
                        avgDefencePower += i.GetComponent<UnitBaseScript>().DefencePowerRatio;
                        avgMovePower += i.GetComponent<UnitBaseScript>().MovePowerRatio;
                        isUnit = true;
                    }
                    else if (i.GetComponent<RTSGameObjectBaseScript>() != null)
                    {
                        count++;
                        avgHP += i.GetComponent<RTSGameObjectBaseScript>().HP;
                        maxHP = i.GetComponent<UnitBaseScript>().maxHP;
                        isUnit = false;
                    }
                    else
                    {
                        Debug.LogError("Impossible object type.");
                        return;
                    }
                }
            }
            avgAttackPower /= count;
            avgDefencePower /= count;
            avgMovePower /= count;
            hpdata.value = avgHP / maxHP;
            if (isUnit)
            {
                // Attack
                if (avgAttackPower > 1)
                {
                    AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgAttackPower - 1 + 0.5f));
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                else if (avgAttackPower < 1)
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgAttackPower + 0.5f));
                }
                else
                {
                    AttackUp.color = new Color(1, 1, 1, 0);
                    AttackDown.color = new Color(1, 1, 1, 0);
                }
                // Defence
                if (avgDefencePower > 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgDefencePower - 1 + 0.5f));
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                else if (avgDefencePower < 1)
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgDefencePower + 0.5f));
                }
                else
                {
                    DefenceUp.color = new Color(1, 1, 1, 0);
                    DefenceDown.color = new Color(1, 1, 1, 0);
                }
                // Move
                if (avgMovePower > 1)
                {
                    MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgMovePower - 1 + 0.5f));
                    MoveDown.color = new Color(1, 1, 1, 0);
                }
                else if (avgMovePower < 1)
                {
                    MoveUp.color = new Color(1, 1, 1, 0);
                    MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgMovePower + 0.5f));
                }
                else
                {
                    MoveUp.color = new Color(1, 1, 1, 0);
                    MoveDown.color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                AttackUp.color = new Color(1, 1, 1, 0);
                AttackDown.color = new Color(1, 1, 1, 0);
                DefenceUp.color = new Color(1, 1, 1, 0);
                DefenceDown.color = new Color(1, 1, 1, 0);
                MoveUp.color = new Color(1, 1, 1, 0);
                MoveDown.color = new Color(1, 1, 1, 0);
            }
            numberCount.text = count.ToString();
        }

        public void SetMainSelected(bool isMainSelected)
        {
            IsMainSelectedGrid = isMainSelected;
            Color temp = GetComponent<Image>().color;
            GetComponent<Image>().color = new Color(temp.r, temp.g, temp.b, isMainSelected ? highlightAlphaValue / 255 : normalAlphaValue / 255);
        }
    }
}