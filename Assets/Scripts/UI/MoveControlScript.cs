using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControlScript : MonoBehaviour
{
    public float verticalSensitive;

    public GameObject navigationUIBasePrefab;
    public GameObject navigationUICirclePrefab;
    public GameObject navigationUILinePrefab;

    private float destinationHorizontalDistance = 0;
    private float destinationVerticalDistance = 0;
    private Vector3 destinationHorizontalPosition = new Vector3();
    private Vector3 destinationHorizontalPositionOffset = new Vector3();

    private GameObject navigationUIBase = null;
    private GameObject navigationUISelfCircle = null;
    private GameObject navigationUIHorizontalCircle = null;
    private GameObject navigationUIVerticalCircle = null;
    private GameObject navigationUILine = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectControlScript.SelectionControlInstance.SelectedOwnGameObjects)
        {
            if (navigationUIBase != null)
            {
                // If exist and get set key, set height
                if (Input.GetKey(InputManager.HotKeys.SetUnitMoveHeight))
                {
                    destinationVerticalDistance += Input.GetAxis("Mouse Y") * verticalSensitive;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();
                    Plane hPlane = new Plane(Vector3.up, center);
                    float distance;
                    if (hPlane.Raycast(ray, out distance))
                    {
                        destinationHorizontalPositionOffset = ray.GetPoint(distance) - destinationHorizontalPosition;
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
                        destinationHorizontalPosition = ray.GetPoint(distance) - destinationHorizontalPositionOffset;
                        destinationHorizontalDistance = (destinationHorizontalPosition - center).magnitude;
                    }
                }
                // If exist and get end key, move
                if (Input.GetKeyDown(InputManager.HotKeys.MoveUnit))
                {
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<RapidPathFinder>() != null)
                        {
                            i.GetComponent<RapidPathFinder>().Move(destinationHorizontalPosition + new Vector3(0, destinationVerticalDistance, 0));
                        }
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
                    if (i.GetComponent<RapidPathFinder>() != null)
                    {
                        i.GetComponent<RapidPathFinder>().Stop();
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

        Cursor.visible = false;
    }

    private void UpdateNavigationUI()
    {
        Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();

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
        destinationHorizontalPositionOffset = new Vector3();

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

        Cursor.visible = true;
    }
}
