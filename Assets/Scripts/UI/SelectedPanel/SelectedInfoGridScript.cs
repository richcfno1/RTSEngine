using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;

namespace RTS.UI.SelectedPanel
{
    public class SelectedInfoGridScript : MonoBehaviour
    {
        public Image icon;
        public Slider hpdata;
        public Text numberCount;
        public Image AttackUp;
        public Image AttackDown;
        public Image DefenceUp;
        public Image DefenceDown;
        public Image MoveUp;
        public Image MoveDown;

        public void UpdateInfoGrid(GameObject targetRTSGameObject)
        {
            hpdata.value = targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().HP / targetRTSGameObject.GetComponent<RTSGameObjectBaseScript>().maxHP;
            UnitBaseScript tempUnitScript = targetRTSGameObject.GetComponent<UnitBaseScript>();
            if (tempUnitScript != null)
            {
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
                        avgAttackPower += i.GetComponent<UnitBaseScript>().AttackPower;
                        avgDefencePower += i.GetComponent<UnitBaseScript>().DefencePower;
                        avgMovePower += i.GetComponent<UnitBaseScript>().MovePower;
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
    }
}