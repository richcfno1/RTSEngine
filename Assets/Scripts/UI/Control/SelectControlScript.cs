using System.Collections.Generic;
using UnityEngine;

public class SelectControlScript : MonoBehaviour
{
    public static SelectControlScript SelectionControlInstance { get; private set; }

    public GameObject SelectionBoxPrefab;

    public Dictionary<string, List<GameObject>> SelectedGameObjects { get; private set; } = new Dictionary<string, List<GameObject>>();
    public bool SelectedOwnUnits { get; private set; } = false;

    private int selfIndex;
    private Vector3 mouseStartPosition;
    private Vector3 mouseEndPosition;
    private bool mouseLeftUp;
    private bool mouseLeftDown;
    private List<GameObject> allSelectableGameObjects;
    private List<Vector3> gameObjectWorldPosition = new List<Vector3>();
    private List<Vector3> gameObjectScreenPosition = new List<Vector3>();

    private GameObject SelectionBox = null;

    void Awake()
    {
        SelectionControlInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        selfIndex = GameManager.GameManagerInstance.selfIndex;
        mouseStartPosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        allSelectableGameObjects = GameManager.GameManagerInstance.GetAllGameObjects();
        if (Input.GetKeyDown(InputManager.HotKeys.SelectUnit) && InputManager.InputManagerInstance.EnableAction)
        {
            mouseStartPosition = Input.mousePosition;
            mouseLeftUp = false;
            mouseLeftDown = true;
            InputManager.InputManagerInstance.CurrentState = InputManager.State.Selecting;
        }
        else if (Input.GetKeyUp(InputManager.HotKeys.SelectUnit) && InputManager.InputManagerInstance.CurrentState == InputManager.State.Selecting)
        {
            mouseEndPosition = Input.mousePosition;
            mouseLeftUp = true;
            mouseLeftDown = false;
            InputManager.InputManagerInstance.CurrentState = InputManager.State.NoAction;
        }
        if (mouseLeftDown)
        {
            // Preparation for select
            gameObjectWorldPosition.Clear();
            gameObjectScreenPosition.Clear();
            foreach (GameObject i in allSelectableGameObjects)
            {
                if (i != null)
                {
                    gameObjectWorldPosition.Add(i.transform.position);
                    gameObjectScreenPosition.Add(Camera.main.WorldToScreenPoint(i.transform.position));
                }
            }

            // Draw box
            Vector3 boxPosition = (Input.mousePosition + mouseStartPosition) / 2;
            float boxSizeX = Mathf.Abs(Input.mousePosition.x - mouseStartPosition.x);
            float boxSizeY = Mathf.Abs(Input.mousePosition.y - mouseStartPosition.y);
            if (SelectionBox == null)
            {
                SelectionBox = Instantiate(SelectionBoxPrefab, transform);
            }
            SelectionBox.transform.position = boxPosition;
            SelectionBox.transform.localScale = new Vector3(boxSizeX, boxSizeY);
        }
        if (mouseLeftUp)
        {
            mouseLeftUp = false;
            Judge();
            Destroy(SelectionBox);
        }

        // Remove destroyed object and add highlighted outline to others
        List<string> toRemoveKey = new List<string>();
        foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
        {
            List<GameObject> toRemoveValue = new List<GameObject>();
            // Check objects with same type
            foreach (GameObject j in i.Value)
            {
                if (j == null)
                {
                    toRemoveValue.Add(j);
                }
            }
            foreach (GameObject j in toRemoveValue)
            {
                i.Value.Remove(j);
            }
            if (i.Value.Count == 0)
            {
                toRemoveKey.Add(i.Key);
            }
        }
        foreach (string i in toRemoveKey)
        {
            SelectedGameObjects.Remove(i);
        }
        //Debug.Log(SelectedGameObjects.Count);
    }

    private GameObject SingleSelectionHelper()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray));
        hits.RemoveAll(x => x.collider.GetComponent<RTSGameObjectBaseScript>() == null);
        if (hits.Count == 0)
        {
            return null;
        }
        else if (hits.Count == 1 || !hits[0].collider.CompareTag("Ship"))
        {
            return hits[0].collider.gameObject;
        }
        else
        {
            if (hits[1].collider.CompareTag("Subsystem"))
            {
                return hits[1].collider.gameObject;
            }
            return hits[0].collider.gameObject;
        }
    }

    private void Judge()
    {
        ClearSelectedGameObjects();
        if (mouseStartPosition != mouseEndPosition)
        {
            for (int i = 0; i < allSelectableGameObjects.Count; i++)
            {
                Vector2 position2D = gameObjectScreenPosition[i];
                if ((position2D.x >= mouseStartPosition.x && position2D.x <= mouseEndPosition.x && position2D.y >= mouseStartPosition.y && position2D.y <= mouseEndPosition.y) ||
                    (position2D.x >= mouseStartPosition.x && position2D.x <= mouseEndPosition.x && position2D.y <= mouseStartPosition.y && position2D.y >= mouseEndPosition.y) ||
                    (position2D.x <= mouseStartPosition.x && position2D.x >= mouseEndPosition.x && position2D.y >= mouseStartPosition.y && position2D.y <= mouseEndPosition.y) ||
                    (position2D.x <= mouseStartPosition.x && position2D.x >= mouseEndPosition.x && position2D.y <= mouseStartPosition.y && position2D.y >= mouseEndPosition.y))
                {
                    if (allSelectableGameObjects[i].GetComponent<UnitBaseScript>() != null && 
                        selfIndex == allSelectableGameObjects[i].GetComponent<UnitBaseScript>().BelongTo)
                    {
                        AddGameObject(allSelectableGameObjects[i]);
                        SelectedOwnUnits = true;
                    }
                }
            }
        }
        // Single selection
        else
        {
            GameObject selected = SingleSelectionHelper();
            if (selected != null)
            {
                AddGameObject(selected);
                SelectedOwnUnits = selected.GetComponent<RTSGameObjectBaseScript>().BelongTo == selfIndex
                    && selected.GetComponent<UnitBaseScript>() != null;
            }
        }
    }

    public List<GameObject> GetAllGameObjects()
    {
        List<GameObject> result = new List<GameObject>();
        foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
        {
            // Check objects with same type
            foreach (GameObject j in i.Value)
            {
                result.Add(j);
            }
        }
        return result;
    }

    public void AddGameObject(GameObject obj)
    {
        if (obj == null || obj.GetComponent<RTSGameObjectBaseScript>() == null)
        {
            return;
        }
        string type = obj.GetComponent<RTSGameObjectBaseScript>().typeID;
        if (SelectedGameObjects.ContainsKey(type))
        {
            SelectedGameObjects[type].Add(obj);
        }
        else
        {
            SelectedGameObjects[type] = new List<GameObject>() { obj };
        }
        obj.AddComponent<cakeslice.Outline>();
    }

    private void ClearSelectedGameObjects()
    {
        foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
        {
            // Check objects with same type
            foreach (GameObject j in i.Value)
            {
                if (j.GetComponent<cakeslice.Outline>() != null)
                {
                    Destroy(j.GetComponent<cakeslice.Outline>());
                }
            }
        }
        SelectedGameObjects.Clear();
        SelectedOwnUnits = false;
    }

    public Vector3 FindCenter()
    {
        Vector3 result = new Vector3();
        int count = 0;
        foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
        {
            // Check objects with same type
            foreach (GameObject j in i.Value)
            {
                result += j.transform.position;
                count++;
            }
        }
        if (count == 0)
        {
            result.x = Mathf.Infinity;
            return result;
        }
        else
        {
            return result / count;
        }
    }
}
