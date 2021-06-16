using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarrierControlPanelScript : MonoBehaviour
{
    public GameObject fighterStatusList;
    public GameObject productList;
    public GameObject fighterStatusGridPrefab;
    public GameObject productGridPrefab;

    private List<GameObject> allFighterStatusGrids = new List<GameObject>();
    private List<GameObject> allProductGrids = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
        {
            GameObject carrier = SelectControlScript.SelectionControlInstance.GetAllGameObjects().
                FirstOrDefault(x => x != null && x.GetComponent<CarrierAbilityScript>() != null);
            if (carrier != default)
            {
                // Enable panel
                GetComponent<CanvasGroup>().alpha = 1;
                GetComponent<CanvasGroup>().interactable = true;
                if (!InputManager.InputManagerInstance.notSelectUI.Contains(gameObject))
                {
                    InputManager.InputManagerInstance.notSelectUI.Add(gameObject);
                }
                CarrierAbilityScript carrierScript = carrier.GetComponent<CarrierAbilityScript>();
                if (carrierScript.SupportedBy.Count != 1)
                {
                    Debug.LogError("Assertion failed: carrier ability supported by incorrect number of subsystem.");
                }
                CarrierSubsystemBaseScript carrierSubsystem = (CarrierSubsystemBaseScript)carrierScript.SupportedBy[0];

                // Status
                int count = 0;
                int existedGrid = allFighterStatusGrids.Count;
                foreach (GameObject i in carrierSubsystem.deployedUnits)
                {
                    if (i == null)
                    {
                        continue;
                    }
                    GameObject temp;
                    if (count < existedGrid)
                    {
                        temp = allFighterStatusGrids[count];
                    }
                    else
                    {
                        temp = Instantiate(fighterStatusGridPrefab, fighterStatusList.transform);
                        allFighterStatusGrids.Add(temp);
                    }
                    Sprite icon = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                        gameObjectLibrary[i.GetComponent<RTSGameObjectBaseScript>().typeID]).icon;
                    temp.GetComponent<Image>().sprite = icon;
                    temp.GetComponentInChildren<Text>().text = "";
                    temp.GetComponent<Button>().onClick.RemoveAllListeners();
                    temp.GetComponent<Button>().onClick.AddListener(
                        () => { SelectControlScript.SelectionControlInstance.SetSelectedGameObjects(new List<GameObject>() { i }); });
                    count++;
                }
                foreach (KeyValuePair<string, int> i in carrierSubsystem.carriedUnits)
                {
                    for (int j = 0; j < i.Value; j++)
                    {
                        GameObject temp;
                        if (count < existedGrid)
                        {
                            temp = allFighterStatusGrids[count];
                        }
                        else
                        {
                            temp = Instantiate(fighterStatusGridPrefab, fighterStatusList.transform);
                            allFighterStatusGrids.Add(temp);
                        }
                        Sprite icon = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                            gameObjectLibrary[GameManager.GameManagerInstance.unitLibrary[i.Key].baseTypeName]).icon;
                        temp.GetComponent<Image>().sprite = icon;
                        temp.GetComponentInChildren<Text>().text = "Undeployed";
                        temp.GetComponent<Button>().onClick.RemoveAllListeners();
                        temp.GetComponent<Button>().onClick.AddListener(
                            () => { carrierScript.UseAbility(new List<object>() { CarrierAbilityScript.UseType.Deploy, i.Key}); });
                        count++;
                    }
                }
               while (count < carrierSubsystem.carrierVolume)
                {
                    GameObject temp;
                    if (count < existedGrid)
                    {
                        temp = allFighterStatusGrids[count];
                    }
                    else
                    {
                        temp = Instantiate(fighterStatusGridPrefab, fighterStatusList.transform);
                        allFighterStatusGrids.Add(temp);
                    }
                    temp.GetComponent<Image>().sprite = fighterStatusGridPrefab.GetComponent<Image>().sprite;
                    temp.GetComponentInChildren<Text>().text = "";
                    temp.GetComponent<Button>().onClick.RemoveAllListeners();
                    count++;
                }

                // Product
                count = 0;
                existedGrid = allProductGrids.Count;
                foreach (string i in carrierSubsystem.products)
                {
                    GameObject temp;
                    if (count < existedGrid)
                    {
                        temp = allProductGrids[count];
                    }
                    else
                    {
                        temp = Instantiate(productGridPrefab, productList.transform);
                        allProductGrids.Add(temp);
                    }
                    Sprite icon = Resources.Load<RTSGameObjectData>(GameManager.GameManagerInstance.
                            gameObjectLibrary[GameManager.GameManagerInstance.unitLibrary[i].baseTypeName]).icon;
                    temp.GetComponent<Image>().sprite = icon;
                    temp.GetComponent<Button>().onClick.RemoveAllListeners();
                    temp.GetComponent<Button>().onClick.AddListener(
                        () => { carrierScript.UseAbility(new List<object>() { CarrierAbilityScript.UseType.Produce, i }); });
                }
                return;
            }
        }
        // Disable panel
        GetComponent<CanvasGroup>().alpha = 0;
        GetComponent<CanvasGroup>().interactable = false;
        InputManager.InputManagerInstance.notSelectUI.Remove(gameObject);
        foreach (GameObject i in allFighterStatusGrids)
        {
            Destroy(i);
        }
        allFighterStatusGrids.Clear();
        foreach (GameObject i in allProductGrids)
        {
            Destroy(i);
        }
        allProductGrids.Clear();
    }
}
