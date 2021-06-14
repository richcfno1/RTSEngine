using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedPanelScript : MonoBehaviour
{
    public GameObject gridPrefab;

    private Dictionary<int, InfoGridScript> allGrids = new Dictionary<int, InfoGridScript>(); // RTSGO index -> Grid
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> allSelected = SelectControlScript.SelectionControlInstance.GetAllGameObjects();
        List<int> allIndex = new List<int>();
        foreach (GameObject i in allSelected)
        {
            RTSGameObjectBaseScript tempScript = i.GetComponent<RTSGameObjectBaseScript>();
            int tempIndex = tempScript.Index;
            allIndex.Add(tempIndex);
            if (allGrids.ContainsKey(tempIndex))
            {
                allGrids[tempIndex].hpdata.value = tempScript.HP / tempScript.maxHP;
                UnitBaseScript tempUnitScript = tempScript.GetComponent<UnitBaseScript>();
                if (tempUnitScript != null)
                {
                    // Attack
                    if (tempUnitScript.AttackPower > 1)
                    {
                        allGrids[tempIndex].AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.AttackPower - 1 + 0.5f));
                        allGrids[tempIndex].AttackDown.color = new Color(1, 1, 1, 0);
                    }
                    else if (tempUnitScript.AttackPower < 1)
                    {
                        allGrids[tempIndex].AttackUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.AttackPower + 0.5f));
                    }
                    else
                    {
                        allGrids[tempIndex].AttackUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].AttackDown.color = new Color(1, 1, 1, 0);
                    }
                    // Defence
                    if (tempUnitScript.DefencePower > 1)
                    {
                        allGrids[tempIndex].DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.DefencePower - 1 + 0.5f));
                        allGrids[tempIndex].DefenceDown.color = new Color(1, 1, 1, 0);
                    }
                    else if (tempUnitScript.DefencePower < 1)
                    {
                        allGrids[tempIndex].DefenceUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.DefencePower + 0.5f));
                    }
                    else
                    {
                        allGrids[tempIndex].DefenceUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].DefenceDown.color = new Color(1, 1, 1, 0);
                    }
                    // Move
                    if (tempUnitScript.MovePower > 1)
                    {
                        allGrids[tempIndex].MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.MovePower - 1 + 0.5f));
                        allGrids[tempIndex].MoveDown.color = new Color(1, 1, 1, 0);
                    }
                    else if (tempUnitScript.MovePower < 1)
                    {
                        allGrids[tempIndex].MoveUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.MovePower + 0.5f));
                    }
                    else
                    {
                        allGrids[tempIndex].MoveUp.color = new Color(1, 1, 1, 0);
                        allGrids[tempIndex].MoveDown.color = new Color(1, 1, 1, 0);
                    }
                }
                else
                {
                    allGrids[tempIndex].AttackUp.color = new Color(1, 1, 1, 0);
                    allGrids[tempIndex].AttackDown.color = new Color(1, 1, 1, 0);
                    allGrids[tempIndex].DefenceUp.color = new Color(1, 1, 1, 0);
                    allGrids[tempIndex].DefenceDown.color = new Color(1, 1, 1, 0);
                    allGrids[tempIndex].MoveUp.color = new Color(1, 1, 1, 0);
                    allGrids[tempIndex].MoveDown.color = new Color(1, 1, 1, 0);
                }
                allGrids[tempIndex].otherData.text = "What should I display?";
            }
            else
            {
                RTSGameObjectData data = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.gameObjectLibrary[tempScript.typeID]);
                InfoGridScript newGridScript = Instantiate(gridPrefab, transform).GetComponent<InfoGridScript>();
                newGridScript.icon.sprite = data.icon;
                newGridScript.hpdata.value = tempScript.HP / tempScript.maxHP;
                newGridScript.otherData.text = "What should I display?";
                allGrids.Add(tempIndex, newGridScript);
            }
        }
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, InfoGridScript> i in allGrids)
        {
            if (!allIndex.Contains(i.Key))
            {
                Destroy(i.Value.gameObject);
                toRemove.Add(i.Key);
            }
        }
        foreach (int i in toRemove)
        {
            allGrids.Remove(i);
        }
    }
}
