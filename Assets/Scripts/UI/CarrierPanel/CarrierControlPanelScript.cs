using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarrierControlPanelScript : MonoBehaviour
{
    public GameObject carrierStatusGridPrefab;
    public Transform carrierStatusList;

    private List<CarrierSubsystemBaseScript> allCarrierSubsystemScripts = new List<CarrierSubsystemBaseScript>();
    private List<CarrierAbilityScript> allCarrierAbilityScripts = new List<CarrierAbilityScript>();
    private Dictionary<string, List<GameObject>> deployedList = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, int> carriedList = new Dictionary<string, int>();
    private Dictionary<string, int> capacityList = new Dictionary<string, int>();

    private Dictionary<string, GameObject> allCarrierStatusGrids = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
        {
            allCarrierSubsystemScripts.Clear();
            allCarrierAbilityScripts.Clear();
            SelectControlScript.SelectionControlInstance.GetAllGameObjects().
                FindAll(x => x != null && x.GetComponent<CarrierAbilityScript>() != null).
                ForEach(x =>
                {
                    allCarrierAbilityScripts.Add(x.GetComponent<CarrierAbilityScript>());
                    foreach (SubsystemBaseScript i in x.GetComponent<CarrierAbilityScript>().SupportedBy)
                    {
                        allCarrierSubsystemScripts.Add((CarrierSubsystemBaseScript)i);
                    }
                });

            if (allCarrierSubsystemScripts.Count != 0)
            {
                // Enable panel
                GetComponent<CanvasGroup>().alpha = 1;
                GetComponent<CanvasGroup>().interactable = true;
                if (!InputManager.InputManagerInstance.notSelectUI.Contains(gameObject))
                {
                    InputManager.InputManagerInstance.notSelectUI.Add(gameObject);
                }

                // Collect data
                deployedList.Clear();
                carriedList.Clear();
                capacityList.Clear();
                float maxDeployProgress = 0;
                float maxProduceProgress = 0;
                foreach (CarrierSubsystemBaseScript i in allCarrierSubsystemScripts)
                {
                    foreach (KeyValuePair<string, List<GameObject>> j in i.deployedUnits)
                    {
                        if (!deployedList.ContainsKey(j.Key))
                        {
                            deployedList.Add(j.Key, new List<GameObject>(j.Value));
                        }
                        else
                        {
                            deployedList[j.Key].AddRange(j.Value);
                        }
                    }
                    foreach (KeyValuePair<string, int> j in i.carriedUnits)
                    {
                        if (!carriedList.ContainsKey(j.Key))
                        {
                            carriedList.Add(j.Key, j.Value);
                        }
                        else
                        {
                            carriedList[j.Key] += j.Value;
                        }
                    }
                    foreach (string j in i.products)
                    {
                        if (!capacityList.ContainsKey(j))
                        {
                            capacityList.Add(j, i.carrierCapacity);
                        }
                        else
                        {
                            capacityList[j] += i.carrierCapacity;
                        }
                    }
                    maxDeployProgress = Mathf.Max(maxDeployProgress, i.DeployProgress);
                    maxProduceProgress = Mathf.Max(maxProduceProgress, i.ProduceProgress);
                }
                List<string> allTypes = new List<string>();
                allTypes.AddRange(deployedList.Keys);
                allTypes.AddRange(carriedList.Keys);
                allTypes.AddRange(capacityList.Keys);
                allTypes = allTypes.Distinct().ToList();

                // Draw grid
                foreach (string i in allTypes)
                {
                    GameObject temp;
                    if (allCarrierStatusGrids.ContainsKey(i))
                    {
                        temp = allCarrierStatusGrids[i];
                    }
                    else
                    {
                        temp = Instantiate(carrierStatusGridPrefab, carrierStatusList);
                        Sprite icon = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                            gameObjectLibrary[GameManager.GameManagerInstance.unitLibrary[i].baseTypeName]).icon;
                        temp.GetComponent<CarrierStatusGridScript>().InitStatusGrid(i, icon, this);
                        allCarrierStatusGrids.Add(i, temp);
                    }
                    temp.GetComponent<CarrierStatusGridScript>().UpdateStatusGrid(
                        deployedList.ContainsKey(i) ? deployedList[i].Count : 0,
                        carriedList.ContainsKey(i) ? carriedList[i] : 0,
                        capacityList.ContainsKey(i) ? capacityList[i] : 0,
                        maxDeployProgress,
                        maxProduceProgress);
                }
                return;
            }
        }
        // Disable panel
        GetComponent<CanvasGroup>().alpha = 0;
        GetComponent<CanvasGroup>().interactable = false;
        InputManager.InputManagerInstance.notSelectUI.Remove(gameObject);
        foreach (KeyValuePair<string, GameObject> i in allCarrierStatusGrids)
        {
            Destroy(i.Value);
        }
        allCarrierStatusGrids.Clear();
    }

    public void SelectUnitsByType(string type)
    {
        SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(deployedList[type]);
    }
    
    public void DepolyUnitsByType(string type)
    {
        foreach (CarrierAbilityScript i in allCarrierAbilityScripts)
        {
            if (i.UseAbility(new List<object>() { CarrierAbilityScript.UseType.Deploy, type }))
            {
                return;
            }
        }
    }

    // TODO: command of 4 buttons
    public void OnDepolyAllButtonClicked()
    {
        foreach (KeyValuePair<string, int> i in carriedList)
        {
            foreach (CarrierAbilityScript j in allCarrierAbilityScripts)
            {
                while (j.UseAbility(new List<object>() { CarrierAbilityScript.UseType.Deploy, i.Key })) ;
            }
        }
    }

    public void OnRecallAllButtonClicked()
    {
        Debug.Log("Unimplemented function: OnRecallAllButtonClicked");
    }

    public void OnSelecteAllButtonClicked()
    {
        List<GameObject> temp = new List<GameObject>();
        foreach (KeyValuePair<string, List<GameObject>> i in deployedList)
        {
            temp.AddRange(i.Value);
        }
        SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(temp);
    }

    public void OnProduceAllButtonClicked()
    {
        foreach (KeyValuePair<string, int> i in capacityList)
        {
            foreach (CarrierAbilityScript j in allCarrierAbilityScripts)
            {
                while (j.UseAbility(new List<object>() { CarrierAbilityScript.UseType.Produce, i.Key })) ;
            }
        }
    }
}
