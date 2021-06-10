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
