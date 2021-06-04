using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cakeslice;

public class SelectControlScript : MonoBehaviour
{
    public static SelectControlScript SelectionControlInstance { get; private set; }

    public int selfIndex;
    public GameObject SelectionBoxPrefab;

    public Dictionary<string, List<GameObject>> SelectedGameObjects { get; private set; } = new Dictionary<string, List<GameObject>>();
    public bool SelectedOwnGameObjects { get; private set; } = false;

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
        mouseStartPosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        allSelectableGameObjects = GameManager.GameManagerInstance.GetAllGameObjects();
        if (mouseLeftDown)
        {
            // Preparation for select
            gameObjectWorldPosition.Clear();
            gameObjectScreenPosition.Clear();
            foreach (GameObject i in allSelectableGameObjects)
            {
                gameObjectWorldPosition.Add(i.transform.position);
                gameObjectScreenPosition.Add(Camera.main.WorldToScreenPoint(i.transform.position));
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
        foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
        {
            // Check objects with same type
            foreach (GameObject j in i.Value)
            {
                if (j == null)
                {
                    i.Value.Remove(j);
                }
            }
            if (i.Value.Count == 0)
            {
                SelectedGameObjects.Remove(i.Key);
            }
        }
        //Debug.Log(SelectedGameObjects.Count);
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
                    if (allSelectableGameObjects[i].GetComponent<ShipBaseScript>() != null && 
                        selfIndex == allSelectableGameObjects[i].GetComponent<ShipBaseScript>().BelongTo)
                    {
                        AddGameObject(allSelectableGameObjects[i]);
                        SelectedOwnGameObjects = true;
                    }
                }
            }
        }
        // Single selection
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Do not check belonging because single selection can select game object which is not belong to "self"
                if (hit.collider.gameObject.GetComponent<GameObjectBaseScript>() != null)
                {
                    AddGameObject(hit.collider.gameObject);
                    SelectedOwnGameObjects = hit.collider.gameObject.GetComponent<GameObjectBaseScript>().BelongTo == selfIndex;
                }
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
        if (obj == null || obj.GetComponent<GameObjectBaseScript>() == null)
        {
            return;
        }
        string type = obj.GetComponent<GameObjectBaseScript>().typeID;
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
        SelectedOwnGameObjects = false;
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

    public void StartSelect()
    {
        mouseStartPosition = Input.mousePosition;
        mouseLeftUp = false;
        mouseLeftDown = true;
    }

    public void EndSelect()
    {
        mouseEndPosition = Input.mousePosition;
        mouseLeftUp = true;
        mouseLeftDown = false;
    }
}
