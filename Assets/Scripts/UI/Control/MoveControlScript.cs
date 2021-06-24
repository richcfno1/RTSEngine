using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using RTS.Ability;
using RTS.RTSGameObject;

namespace RTS.UI.Control
{
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
            if (InputManager.InputManagerInstance.CurrentState != InputManager.State.NoAction &&
                InputManager.InputManagerInstance.CurrentState != InputManager.State.Moving)
            {
                return;
            }
            if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                // This must be run first, or at the init time, set destination will also be called due to same keydown
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
                    // If exist and get set height key, set height
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
                            i.Key.UseAbility(new List<object>() { MoveAbilityScript.ActionType.MoveTo, i.Value });
                        }
                        ClearNavigationUI();
                    }
                    UpdateNavigationUI();
                }
                // If not exist, create, this is the first time of move control, so test where the cursor is and if it is able to do such action
                else if (navigationUIBase == null && Input.GetKeyDown(InputManager.HotKeys.MoveUnit) &&
                    InputManager.InputManagerInstance.EnableAction && SingleSelectionHelper() == null)
                {
                    CreateNavigationUI();
                }

                // If exist and get stop key, stop (this does not require navigation UI)
                if (Input.GetKeyDown(InputManager.HotKeys.StopUnit))
                {
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<MoveAbilityScript>() != null)
                        {
                            i.GetComponent<MoveAbilityScript>().UseAbility(new List<object>() { MoveAbilityScript.ActionType.Stop, Vector3.zero });
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
            InputManager.InputManagerInstance.CurrentState = InputManager.State.Moving;
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
            Cursor.visible = true;
            InputManager.InputManagerInstance.CurrentState = InputManager.State.NoAction;
        }

        private Dictionary<MoveAbilityScript, Vector3> FindDestination(List<MoveAbilityScript> allAgents, Vector3 destination, Vector3 forwardDirection)
        {
            Dictionary<MoveAbilityScript, Vector3> temp = new Dictionary<MoveAbilityScript, Vector3>();
            Vector3 destinationForEachClass = destination;
            Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;

            List<MoveAbilityScript> allFighters = allAgents.FindAll(x => x.Host.objectScale == RTSGameObjectBaseScript.ObjectScale.Fighter);
            if (allFighters.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 10;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFighters = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (MoveAbilityScript i in allFighters)
                {
                    destinationForFighters.Add(tempDestination);
                    tempDestination += right * 20;
                }
                Vector3 destinationForFightersCenter = new Vector3();
                foreach (Vector3 i in destinationForFighters)
                {
                    destinationForFightersCenter += i;
                }
                Vector3 offset = destinationForEachClass - destinationForFightersCenter / destinationForFighters.Count;
                List<Vector3> trueDestinationForFighters = new List<Vector3>();
                foreach (Vector3 i in destinationForFighters)
                {
                    trueDestinationForFighters.Add(i + offset);
                }
                foreach (MoveAbilityScript i in allFighters)
                {
                    Vector3 destinationForThis = new Vector3();
                    float distance = Mathf.Infinity;
                    foreach (Vector3 j in trueDestinationForFighters)
                    {
                        float tempDistance = (i.transform.position - j).magnitude;
                        if (tempDistance < distance)
                        {
                            distance = tempDistance;
                            destinationForThis = j;
                        }
                    }
                    temp.Add(i, destinationForThis);
                    trueDestinationForFighters.Remove(destinationForThis);
                }
                destinationForEachClass -= forwardDirection.normalized * 10;
            }

            List<MoveAbilityScript> allFrigates = allAgents.FindAll(x => x.Host.objectScale == RTSGameObjectBaseScript.ObjectScale.Frigate);
            if (allFrigates.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 30;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFrigates = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (MoveAbilityScript i in allFrigates)
                {
                    destinationForFrigates.Add(tempDestination);
                    tempDestination += right * 60;
                }
                Vector3 destinationForFrigatesCenter = new Vector3();
                foreach (Vector3 i in destinationForFrigates)
                {
                    destinationForFrigatesCenter += i;
                }
                Vector3 offset = destinationForEachClass - destinationForFrigatesCenter / destinationForFrigates.Count;
                List<Vector3> trueDestinationForFrigates = new List<Vector3>();
                foreach (Vector3 i in destinationForFrigates)
                {
                    trueDestinationForFrigates.Add(i + offset);
                }
                foreach (MoveAbilityScript i in allFrigates)
                {
                    Vector3 destinationForThis = new Vector3();
                    float distance = Mathf.Infinity;
                    foreach (Vector3 j in trueDestinationForFrigates)
                    {
                        float tempDistance = (i.transform.position - j).magnitude;
                        if (tempDistance < distance)
                        {
                            distance = tempDistance;
                            destinationForThis = j;
                        }
                    }
                    temp.Add(i, destinationForThis);
                    trueDestinationForFrigates.Remove(destinationForThis);
                }
                destinationForEachClass -= forwardDirection.normalized * 30;
            }

            List<MoveAbilityScript> allCruisers = allAgents.FindAll(x => x.Host.objectScale == RTSGameObjectBaseScript.ObjectScale.Cruiser);
            if (allCruisers.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 50;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForCruisers = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (MoveAbilityScript i in allCruisers)
                {
                    destinationForCruisers.Add(tempDestination);
                    tempDestination += right * 100;
                }
                Vector3 destinationForCruisersCenter = new Vector3();
                foreach (Vector3 i in destinationForCruisers)
                {
                    destinationForCruisersCenter += i;
                }
                Vector3 offset = destinationForEachClass - destinationForCruisersCenter / destinationForCruisers.Count;
                List<Vector3> trueDestinationForcruisers = new List<Vector3>();
                foreach (Vector3 i in destinationForCruisers)
                {
                    trueDestinationForcruisers.Add(i + offset);
                }
                foreach (MoveAbilityScript i in allCruisers)
                {
                    Vector3 destinationForThis = new Vector3();
                    float distance = Mathf.Infinity;
                    foreach (Vector3 j in trueDestinationForcruisers)
                    {
                        float tempDistance = (i.transform.position - j).magnitude;
                        if (tempDistance < distance)
                        {
                            distance = tempDistance;
                            destinationForThis = j;
                        }
                    }
                    temp.Add(i, destinationForThis);
                    trueDestinationForcruisers.Remove(destinationForThis);
                }
                destinationForEachClass -= forwardDirection.normalized * 50;
            }

            List<MoveAbilityScript> allBattleships = allAgents.FindAll(x => x.Host.objectScale == RTSGameObjectBaseScript.ObjectScale.Battleship);
            if (allBattleships.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 80;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForBattleships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (MoveAbilityScript i in allBattleships)
                {
                    destinationForBattleships.Add(tempDestination);
                    tempDestination += right * 160;
                }
                Vector3 destinationForBattleshipsCenter = new Vector3();
                foreach (Vector3 i in destinationForBattleships)
                {
                    destinationForBattleshipsCenter += i;
                }
                Vector3 offset = destinationForEachClass - destinationForBattleshipsCenter / destinationForBattleships.Count;
                List<Vector3> trueDestinationForBattleships = new List<Vector3>();
                foreach (Vector3 i in destinationForBattleships)
                {
                    trueDestinationForBattleships.Add(i + offset);
                }
                foreach (MoveAbilityScript i in allBattleships)
                {
                    Vector3 destinationForThis = new Vector3();
                    float distance = Mathf.Infinity;
                    foreach (Vector3 j in trueDestinationForBattleships)
                    {
                        float tempDistance = (i.transform.position - j).magnitude;
                        if (tempDistance < distance)
                        {
                            distance = tempDistance;
                            destinationForThis = j;
                        }
                    }
                    temp.Add(i, destinationForThis);
                    trueDestinationForBattleships.Remove(destinationForThis);
                }
                destinationForEachClass -= forwardDirection.normalized * 80;
            }

            List<MoveAbilityScript> allMotherships = allAgents.FindAll(x => x.Host.objectScale == RTSGameObjectBaseScript.ObjectScale.Mothership);
            if (allMotherships.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 120;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForMotherships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (MoveAbilityScript i in allMotherships)
                {
                    destinationForMotherships.Add(tempDestination);
                    tempDestination += right * 240;
                }
                Vector3 destinationForMothershipsCenter = new Vector3();
                foreach (Vector3 i in destinationForMotherships)
                {
                    destinationForMothershipsCenter += i;
                }
                Vector3 offset = destinationForEachClass - destinationForMothershipsCenter / destinationForMotherships.Count;
                List<Vector3> trueDestinationForMotherships = new List<Vector3>();
                foreach (Vector3 i in destinationForMotherships)
                {
                    trueDestinationForMotherships.Add(i + offset);
                }
                foreach (MoveAbilityScript i in allMotherships)
                {
                    Vector3 destinationForThis = new Vector3();
                    float distance = Mathf.Infinity;
                    foreach (Vector3 j in trueDestinationForMotherships)
                    {
                        float tempDistance = (i.transform.position - j).magnitude;
                        if (tempDistance < distance)
                        {
                            distance = tempDistance;
                            destinationForThis = j;
                        }
                    }
                    temp.Add(i, destinationForThis);
                    trueDestinationForMotherships.Remove(destinationForThis);
                }
                destinationForEachClass -= forwardDirection.normalized * 120;
            }

            return temp;
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
    }
}
