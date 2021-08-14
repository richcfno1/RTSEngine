using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.UI.Command;
using RTS.RTSGameObject;

namespace RTS.UI.SelectedPanel
{
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
        private SelectedDetailGridScript selectedDetaillGrid;
        private Dictionary<int, SelectedInfoGridScript> allGridsByIndex = new Dictionary<int, SelectedInfoGridScript>();      // RTSGO index -> Grid
        private Dictionary<string, SelectedInfoGridScript> allGridsByType = new Dictionary<string, SelectedInfoGridScript>(); // RTSGO type -> Grid

        // Start is called before the first frame update
        void Start()
        {
            lastSelectedType = SelectedType.Empty;
        }

        // Update is called once per frame
        void Update()
        {
            SortedDictionary<string, List<GameObject>> allSelected = SelectControlScript.SelectionControlInstance.SelectedGameObjects;
            List<GameObject> allSelectedList = SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList();
            if (allSelected.Count == 1)
            {
                if (allSelectedList.Count == 1)
                {
                    GetComponent<GridLayoutGroup>().cellSize = new Vector2(480, 400);
                    if (lastSelectedType != SelectedType.Single)
                    {
                        ClearAll();
                    }
                    if (allSelectedList[0] != null)
                    {
                        if (selectedDetaillGrid == null)
                        {
                            selectedDetaillGrid = Instantiate(detailGridPrefab, transform).GetComponent<SelectedDetailGridScript>();
                        }
                        selectedDetaillGrid.icon.sprite = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                            gameObjectLibrary[allSelectedList[0].GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                        selectedDetaillGrid.UpdateDetailGrid(allSelectedList[0]);
                        lastSelectedType = SelectedType.Single;
                    }
                    else
                    {
                        ClearAll();
                    }
                }
                else
                {
                    GetComponent<GridLayoutGroup>().cellSize = new Vector2(120, 40);
                    if (lastSelectedType != SelectedType.SameType)
                    {
                        ClearAll();
                    }
                    List<int> allIndex = new List<int>();
                    foreach (GameObject i in allSelectedList)
                    {
                        if (i == null)
                        {
                            continue;
                        }
                        int index = i.GetComponent<RTSGameObjectBaseScript>().Index;
                        if (allGridsByIndex.ContainsKey(index))
                        {
                            allGridsByIndex[index].UpdateInfoGrid(i);
                        }
                        else
                        {
                            GameObject temp = Instantiate(infoGridPrefab, transform);
                            temp.GetComponent<SelectedInfoGridScript>().icon.sprite = Resources.Load<RTSGameObjectData>(
                                GameManager.GameManagerInstance.gameObjectLibrary[i.GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                            temp.GetComponent<Button>().onClick.AddListener(
                                () => { SelectControlScript.SelectionControlInstance.SetMainSelectedGameObject(i); });
                            temp.GetComponent<SelectedInfoGridScript>().UpdateInfoGrid(i);
                            allGridsByIndex.Add(index, temp.GetComponent<SelectedInfoGridScript>());
                        }
                        allIndex.Add(index);
                        if (i == SelectControlScript.SelectionControlInstance.MainSelectedGameObject)
                        {
                            if (!allGridsByIndex[index].IsMainSelectedGrid)
                            {
                                allGridsByIndex[index].SetMainSelected(true);
                                allGridsByIndex[index].GetComponent<Button>().onClick.RemoveAllListeners();
                                allGridsByIndex[index].GetComponent<Button>().onClick.AddListener(
                                    () => { SelectControlScript.SelectionControlInstance.SetSelectedGameObjects( new List<GameObject>() { i }); });
                            }
                        }
                        else
                        {
                            if (allGridsByIndex[index].IsMainSelectedGrid)
                            {
                                allGridsByIndex[index].SetMainSelected(false);
                                allGridsByIndex[index].GetComponent<Button>().onClick.RemoveAllListeners();
                                allGridsByIndex[index].GetComponent<Button>().onClick.AddListener(
                                    () => { SelectControlScript.SelectionControlInstance.SetMainSelectedGameObject(i); });
                            }
                        }
                    }
                    List<int> toRemove = new List<int>();
                    foreach (KeyValuePair<int, SelectedInfoGridScript> i in allGridsByIndex)
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
                GetComponent<GridLayoutGroup>().cellSize = new Vector2(120, 40);
                if (lastSelectedType != SelectedType.Multi)
                {
                    ClearAll();
                }
                foreach (KeyValuePair<string, List<GameObject>> i in allSelected)
                {
                    if (allGridsByType.ContainsKey(i.Key))
                    {
                        allGridsByType[i.Key].UpdateInfoGrid(i.Value);
                    }
                    else
                    {
                        GameObject temp = Instantiate(infoGridPrefab, transform);
                        temp.GetComponent<SelectedInfoGridScript>().icon.sprite = Resources.Load<RTSGameObjectData>(
                            GameManager.GameManagerInstance.gameObjectLibrary[GameManager.GameManagerInstance.unitLibrary[i.Key].baseTypeName]).icon;
                        temp.GetComponent<Button>().onClick.AddListener(
                                () => { SelectControlScript.SelectionControlInstance.SetMainSelectedType(i.Key); });
                        temp.GetComponent<SelectedInfoGridScript>().UpdateInfoGrid(i.Value);
                        allGridsByType.Add(i.Key, temp.GetComponent<SelectedInfoGridScript>());
                    }
                    if (i.Key == SelectControlScript.SelectionControlInstance.MainSelectedType)
                    {
                        if (!allGridsByType[i.Key].IsMainSelectedGrid)
                        {
                            allGridsByType[i.Key].SetMainSelected(true);
                            allGridsByType[i.Key].GetComponent<Button>().onClick.RemoveAllListeners();
                            allGridsByType[i.Key].GetComponent<Button>().onClick.AddListener(
                                () => { SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(i.Value); });
                        }
                    }
                    else
                    {
                        if (allGridsByType[i.Key].IsMainSelectedGrid)
                        {
                            allGridsByType[i.Key].SetMainSelected(false);
                            allGridsByType[i.Key].GetComponent<Button>().onClick.RemoveAllListeners();
                            allGridsByType[i.Key].GetComponent<Button>().onClick.AddListener(
                                () => { SelectControlScript.SelectionControlInstance.SetMainSelectedType(i.Key); });
                        }
                    }
                }
                List<string> allTypes = new List<string>(allSelected.Keys);
                List<string> toRemove = new List<string>();
                foreach (KeyValuePair<string, SelectedInfoGridScript> i in allGridsByType)
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

        private void ClearAll()
        {
            // Clear all
            if (lastSelectedType != SelectedType.Empty)
            {
                if (selectedDetaillGrid != null && selectedDetaillGrid.gameObject != null)
                {
                    Destroy(selectedDetaillGrid.gameObject);
                    selectedDetaillGrid = null;
                }
                foreach (KeyValuePair<int, SelectedInfoGridScript> i in allGridsByIndex)
                {
                    if (i.Value != null && i.Value.gameObject != null)
                    {
                        Destroy(i.Value.gameObject);
                    }
                }
                allGridsByIndex.Clear();
                foreach (KeyValuePair<string, SelectedInfoGridScript> i in allGridsByType)
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
}