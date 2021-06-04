using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectControlScript : MonoBehaviour
{
    public static SelectControlScript SelectionControlInstance { get; private set; }

    public int selfIndex;

    public Dictionary<string, List<GameObject>> SelectedGameObjects { get; private set; } = new Dictionary<string, List<GameObject>>();
    public bool SelectedOwnGameObjects { get; private set; } = false;

    private Vector3 mouseStartPosition;
    private Vector3 mouseEndPosition;
    private bool mouseLeftUp;
    private bool mouseLeftDown;
    private List<GameObject> allSelectableGameObjects;
    private List<Vector3> gameObjectWorldPosition = new List<Vector3>();
    private List<Vector3> gameObjectScreenPosition = new List<Vector3>();

    private LineRenderer line;

    void Awake()
    {
        SelectionControlInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mouseStartPosition = Input.mousePosition;
        line = GetComponent<LineRenderer>();
        line.positionCount = 5;
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
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.enabled = true;
            line.SetPosition(0, Camera.main.ScreenToWorldPoint(new Vector3(mouseStartPosition.x, mouseStartPosition.y, 1)));
            line.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, mouseStartPosition.y, 1)));
            line.SetPosition(2, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)));
            line.SetPosition(3, Camera.main.ScreenToWorldPoint(new Vector3(mouseStartPosition.x, Input.mousePosition.y, 1)));
            line.SetPosition(4, Camera.main.ScreenToWorldPoint(new Vector3(mouseStartPosition.x, mouseStartPosition.y, 1)));
        }
        if (mouseLeftUp)
        {
            mouseLeftUp = false;
            Judge();
            line.enabled = false;
        }

        // Remove destroyed object
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

    private void AddGameObject(GameObject obj)
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
    }

    private void Judge()
    {
        SelectedGameObjects.Clear();
        SelectedOwnGameObjects = false;
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
                    if (selfIndex == allSelectableGameObjects[i].GetComponent<GameObjectBaseScript>().BelongTo)
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
