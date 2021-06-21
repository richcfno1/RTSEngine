using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedPanelScript : MonoBehaviour
{
    private enum SelectedType
    {
        Empty,
        Single,
        SameType,
        Multi,
    }

    public GameObject detailGridPrefab;
    public GameObject infoGridPrefab;

    private SelectedType lastSelectedType;
    private DetailGridScript detaillGrid;
    private Dictionary<int, InfoGridScript> allGridsByIndex = new Dictionary<int, InfoGridScript>(); // RTSGO index -> Grid
    private Dictionary<string, InfoGridScript> allGridsByType = new Dictionary<string, InfoGridScript>(); // RTSGO type -> Grid
    // Start is called before the first frame update
    void Start()
    {
        lastSelectedType = SelectedType.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<string, List<GameObject>> allSelected = SelectControlScript.SelectionControlInstance.SelectedGameObjects;
        List<GameObject> allSelectedList = SelectControlScript.SelectionControlInstance.GetAllGameObjects();
        if (allSelected.Count == 1)
        {
            if (allSelectedList.Count == 1)
            {
                GetComponent<GridLayoutGroup>().enabled = false;
                if (lastSelectedType != SelectedType.Single)
                {
                    ClearAll();
                }
                if (detaillGrid == null)
                {
                    detaillGrid = Instantiate(detailGridPrefab, transform).GetComponent<DetailGridScript>();
                }
                detaillGrid.icon.sprite = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                    gameObjectLibrary[allSelectedList[0].GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                UpdateDetailGrid(detaillGrid, allSelectedList[0]);
                lastSelectedType = SelectedType.Single;
            }
            else
            {
                GetComponent<GridLayoutGroup>().enabled = true;
                if (lastSelectedType != SelectedType.SameType)
                {
                    ClearAll();
                }
                List<int> allIndex = new List<int>();
                foreach (GameObject i in allSelectedList)
                {
                    int index = i.GetComponent<RTSGameObjectBaseScript>().Index;
                    if (allGridsByIndex.ContainsKey(index))
                    {
                        UpdateInfoGrid(allGridsByIndex[index], i);
                    }
                    else
                    {
                        GameObject temp = Instantiate(infoGridPrefab, transform);
                        temp.GetComponent<InfoGridScript>().icon.sprite = Resources.Load<RTSGameObjectData>(
                            GameManager.GameManagerInstance.gameObjectLibrary[i.GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                        temp.GetComponent<Button>().onClick.AddListener(
                            () => { SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(new List<GameObject>() { i }); });
                        UpdateInfoGrid(temp.GetComponent<InfoGridScript>(), i);
                        allGridsByIndex.Add(index, temp.GetComponent<InfoGridScript>());
                    }
                    allIndex.Add(index);
                }
                List<int> toRemove = new List<int>();
                foreach (KeyValuePair<int, InfoGridScript> i in allGridsByIndex)
                {
                    if (!allIndex.Contains(i.Key))
                    {
                        if (i.Value != null && i.Value.gameObject != null)
                        {
                            Destroy(i.Value.gameObject);
                        }
                        toRemove.Add(i.Key);
                    }
                }
                foreach (int i in toRemove)
                {
                    allGridsByIndex.Remove(i);
                }
                lastSelectedType = SelectedType.SameType;
            }
        }
        else if (allSelected.Count > 1)
        {
            GetComponent<GridLayoutGroup>().enabled = true;
            if (lastSelectedType != SelectedType.Multi)
            {
                ClearAll();
            }
            foreach (KeyValuePair<string, List<GameObject>> i in allSelected)
            {
                if (allGridsByType.ContainsKey(i.Key))
                {
                    UpdateInfoGrid(allGridsByType[i.Key], i.Value);
                }
                else
                {
                    GameObject temp = Instantiate(infoGridPrefab, transform);
                    temp.GetComponent<InfoGridScript>().icon.sprite = Resources.Load<RTSGameObjectData>(
                        GameManager.GameManagerInstance.gameObjectLibrary[GameManager.GameManagerInstance.unitLibrary[i.Key].baseTypeName]).icon;
                    temp.GetComponent<Button>().onClick.AddListener(
                            () => { SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(i.Value); });
                    UpdateInfoGrid(temp.GetComponent<InfoGridScript>(), i.Value);
                    allGridsByType.Add(i.Key, temp.GetComponent<InfoGridScript>());
                }
            }
            List<string> allTypes = new List<string>(allSelected.Keys);
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, InfoGridScript> i in allGridsByType)
            {
                if (!allTypes.Contains(i.Key))
                {
                    if (i.Value != null && i.Value.gameObject != null)
                    {
                        Destroy(i.Value.gameObject);
                    }
                    toRemove.Add(i.Key);
                }
            }
            foreach (string i in toRemove)
            {
                allGridsByType.Remove(i);
            }
            lastSelectedType = SelectedType.Multi;
        }
        else
        {
            ClearAll();
        }
    }

    private void UpdateDetailGrid(DetailGridScript script, GameObject oneGameObject)
    {
        script.hpdata.value = oneGameObject.GetComponent<RTSGameObjectBaseScript>().HP / oneGameObject.GetComponent<RTSGameObjectBaseScript>().maxHP;
        UnitBaseScript tempUnitScript = oneGameObject.GetComponent<UnitBaseScript>();
        if (tempUnitScript != null)
        {
            script.typeName.text = tempUnitScript.UnitTypeID;
            // Attack
            if (tempUnitScript.AttackPower > 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.AttackPower - 1 + 0.5f));
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.AttackPower < 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.AttackPower + 0.5f));
            }
            else
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            // Defence
            if (tempUnitScript.DefencePower > 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.DefencePower - 1 + 0.5f));
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.DefencePower < 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.DefencePower + 0.5f));
            }
            else
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            // Move
            if (tempUnitScript.MovePower > 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.MovePower - 1 + 0.5f));
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.MovePower < 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.MovePower + 0.5f));
            }
            else
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            script.typeName.text = gameObject.GetComponent<RTSGameObjectBaseScript>().typeID;
            script.AttackUp.color = new Color(1, 1, 1, 0);
            script.AttackDown.color = new Color(1, 1, 1, 0);
            script.DefenceUp.color = new Color(1, 1, 1, 0);
            script.DefenceDown.color = new Color(1, 1, 1, 0);
            script.MoveUp.color = new Color(1, 1, 1, 0);
            script.MoveDown.color = new Color(1, 1, 1, 0);
        }
    }

    private void UpdateInfoGrid(InfoGridScript script, GameObject oneGameObject)
    {
        script.hpdata.value = oneGameObject.GetComponent<RTSGameObjectBaseScript>().HP / oneGameObject.GetComponent<RTSGameObjectBaseScript>().maxHP;
        UnitBaseScript tempUnitScript = oneGameObject.GetComponent<UnitBaseScript>();
        if (tempUnitScript != null)
        {
            // Attack
            if (tempUnitScript.AttackPower > 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.AttackPower - 1 + 0.5f));
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.AttackPower < 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.AttackPower + 0.5f));
            }
            else
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            // Defence
            if (tempUnitScript.DefencePower > 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.DefencePower - 1 + 0.5f));
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.DefencePower < 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.DefencePower + 0.5f));
            }
            else
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            // Move
            if (tempUnitScript.MovePower > 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(tempUnitScript.MovePower - 1 + 0.5f));
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
            else if (tempUnitScript.MovePower < 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - tempUnitScript.MovePower + 0.5f));
            }
            else
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            script.AttackUp.color = new Color(1, 1, 1, 0);
            script.AttackDown.color = new Color(1, 1, 1, 0);
            script.DefenceUp.color = new Color(1, 1, 1, 0);
            script.DefenceDown.color = new Color(1, 1, 1, 0);
            script.MoveUp.color = new Color(1, 1, 1, 0);
            script.MoveDown.color = new Color(1, 1, 1, 0);
        }
        script.numberCount.text = "";
    }

    private void UpdateInfoGrid(InfoGridScript script, List<GameObject> allGameObjects)
    {
        if (script == null)
        {
            return;
        }
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
        script.hpdata.value = avgHP / maxHP;
        if (isUnit)
        {
            // Attack
            if (avgAttackPower > 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgAttackPower - 1 + 0.5f));
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            else if (avgAttackPower < 1)
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgAttackPower + 0.5f));
            }
            else
            {
                script.AttackUp.color = new Color(1, 1, 1, 0);
                script.AttackDown.color = new Color(1, 1, 1, 0);
            }
            // Defence
            if (avgDefencePower > 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgDefencePower - 1 + 0.5f));
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            else if (avgDefencePower < 1)
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgDefencePower + 0.5f));
            }
            else
            {
                script.DefenceUp.color = new Color(1, 1, 1, 0);
                script.DefenceDown.color = new Color(1, 1, 1, 0);
            }
            // Move
            if (avgMovePower > 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, Mathf.Clamp01(avgMovePower - 1 + 0.5f));
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
            else if (avgMovePower < 1)
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, Mathf.Clamp01(1 - avgMovePower + 0.5f));
            }
            else
            {
                script.MoveUp.color = new Color(1, 1, 1, 0);
                script.MoveDown.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            script.AttackUp.color = new Color(1, 1, 1, 0);
            script.AttackDown.color = new Color(1, 1, 1, 0);
            script.DefenceUp.color = new Color(1, 1, 1, 0);
            script.DefenceDown.color = new Color(1, 1, 1, 0);
            script.MoveUp.color = new Color(1, 1, 1, 0);
            script.MoveDown.color = new Color(1, 1, 1, 0);
        }
        script.numberCount.text = count.ToString();
    }

    private void ClearAll()
    {
        // Clear all
        if (lastSelectedType != SelectedType.Empty)
        {
            if (detaillGrid != null && detaillGrid.gameObject != null)
            {
                Destroy(detaillGrid.gameObject);
                detaillGrid = null;
            }
            foreach (KeyValuePair<int, InfoGridScript> i in allGridsByIndex)
            {
                if (i.Value != null && i.Value.gameObject != null)
                {
                    Destroy(i.Value.gameObject);
                }
            }
            allGridsByIndex.Clear();
            foreach (KeyValuePair<string, InfoGridScript> i in allGridsByType)
            {
                if (i.Value != null && i.Value.gameObject != null)
                {
                    Destroy(i.Value.gameObject);
                }
            }
            allGridsByType.Clear();
        }
        lastSelectedType = SelectedType.Empty;
    }
}
