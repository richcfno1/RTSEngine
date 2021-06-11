using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Windows;

public class MoveControlScript : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    public float verticalSensitive;

    public GameObject navigationUIBasePrefab;
    public GameObject navigationUICirclePrefab;
    public GameObject navigationUILinePrefab;

    private float destinationHorizontalDistance = 0;
    private float destinationVerticalDistance = 0;
    private Vector3 destinationHorizontalPosition = new Vector3();

    private GameObject navigationUIBase = null;
    private GameObject navigationUISelfCircle = null;
    private GameObject navigationUIHorizontalCircle = null;
    private GameObject navigationUIVerticalCircle = null;
    private GameObject navigationUILine = null;

    private int mousePositionX = 0;
    private int mousePositionY = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
        {
            if (navigationUIBase != null)
            {
                if (Input.GetKeyDown(InputManager.HotKeys.SetUnitMoveHeight))
                {
                    POINT p;
                    GetCursorPos(out p);
                    mousePositionX = p.X;
                    mousePositionY = p.Y;
                }
                else if (Input.GetKeyUp(InputManager.HotKeys.SetUnitMoveHeight))
                {
                    SetCursorPos(mousePositionX, mousePositionY);
                }
                // If exist and get set key, set height
                if (Input.GetKey(InputManager.HotKeys.SetUnitMoveHeight))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();
                    Plane hPlane = new Plane(Vector3.right, destinationHorizontalPosition);
                    float distance;
                    if (hPlane.Raycast(ray, out distance))
                    {
                        destinationVerticalDistance = (ray.GetPoint(distance) - destinationHorizontalPosition).y;
                    }
                }
                // Or move in horizontal plane
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();
                    Plane hPlane = new Plane(Vector3.up, center);
                    float distance;
                    if (hPlane.Raycast(ray, out distance))
                    {
                        destinationHorizontalPosition = ray.GetPoint(distance);
                        destinationHorizontalDistance = (destinationHorizontalPosition - center).magnitude;
                    }
                }
                // If exist and get end key, move
                if (Input.GetKeyDown(InputManager.HotKeys.MoveUnit))
                {
                    List<MoveAbilityScript> allAgents = new List<MoveAbilityScript>();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<MoveAbilityScript>() != null)
                        {
                            allAgents.Add(i.GetComponent<MoveAbilityScript>());
                        }
                    }
                    Vector3 destination = destinationHorizontalPosition + new Vector3(0, destinationVerticalDistance, 0);
                    foreach (KeyValuePair<MoveAbilityScript, Vector3> i in FindDestination(allAgents, destination, destination - SelectControlScript.SelectionControlInstance.FindCenter()))
                    {
                        i.Key.UseAbility(new List<object>(){ 1, i.Value });
                    }
                    ClearNavigationUI();
                }
                UpdateNavigationUI();
            }
            // If not exist, create
            else if (navigationUIBase == null && Input.GetKeyDown(InputManager.HotKeys.MoveUnit))
            {
                CreateNavigationUI();
            }
            // If exist and get stop key, stop
            if (Input.GetKeyDown(InputManager.HotKeys.StopUnit))
            {
                foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                {
                    if (i.GetComponent<MoveAbilityScript>() != null)
                    {
                        i.GetComponent<MoveAbilityScript>().PauseAbility();
                    }
                }
                return;
            }
        }
        else
        {
            ClearNavigationUI();
        }
    }

    private void CreateNavigationUI()
    {
        navigationUIBase = Instantiate(navigationUIBasePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

        navigationUISelfCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
        navigationUIHorizontalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
        navigationUIVerticalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

        navigationUILine = Instantiate(navigationUILinePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);

        //Cursor.visible = false;
    }

    private void UpdateNavigationUI()
    {
        Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();

        if (center.x == Mathf.Infinity)
        {
            ClearNavigationUI();
            return;
        }

        navigationUIBase.transform.position = center;
        navigationUIBase.transform.localScale = Vector3.one * destinationHorizontalDistance * 2;

        navigationUISelfCircle.transform.position = center;
        navigationUIHorizontalCircle.transform.position = destinationHorizontalPosition;
        navigationUIVerticalCircle.transform.position = destinationHorizontalPosition + new Vector3(0, destinationVerticalDistance, 0);

        // Draw line
        LineRenderer line = navigationUILine.GetComponent<LineRenderer>();
        line.positionCount = 4;
        line.startColor = Color.green;
        line.endColor = Color.green;
        line.enabled = true;
        line.SetPosition(0, center);
        line.SetPosition(1, destinationHorizontalPosition);
        line.SetPosition(2, destinationHorizontalPosition + new Vector3(0, destinationVerticalDistance, 0));
        line.SetPosition(3, center);
    }

    private void ClearNavigationUI()
    {
        destinationHorizontalDistance = 0;
        destinationVerticalDistance = 0;
        destinationHorizontalPosition = new Vector3();

        if (navigationUIBase != null)
        {
            Destroy(navigationUIBase);
        }
        if (navigationUISelfCircle != null)
        {
            Destroy(navigationUISelfCircle);
        }
        if (navigationUIHorizontalCircle != null)
        {
            Destroy(navigationUIHorizontalCircle);
        }
        if (navigationUIVerticalCircle != null)
        {
            Destroy(navigationUIVerticalCircle);
        }
        if (navigationUILine != null)
        {
            Destroy(navigationUILine);
        }
        //Cursor.visible = true;
    }

    private Dictionary<MoveAbilityScript, Vector3> FindDestination(List<MoveAbilityScript> allAgents, Vector3 destination, Vector3 forwardDirection)
    {
        Dictionary<MoveAbilityScript, Vector3> temp = new Dictionary<MoveAbilityScript, Vector3>();
        Vector3 newDestination = destination;
        Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        foreach (MoveAbilityScript agent in allAgents)
        {
            newDestination += right * agent.AgentRadius;
            temp.Add(agent, newDestination);
            newDestination += right * agent.AgentRadius;
        }
        Vector3 newDestinationCenter = new Vector3();
        foreach (Vector3 i in temp.Values)
        {
            newDestinationCenter += i;
        }
        Vector3 offset = destination - newDestinationCenter / temp.Count;
        Dictionary<MoveAbilityScript, Vector3> result = new Dictionary<MoveAbilityScript, Vector3>();
        foreach (KeyValuePair<MoveAbilityScript, Vector3> i in temp)
        {
            result.Add(i.Key, i.Value + offset);
        }
        return result;
    }
}
