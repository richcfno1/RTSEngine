using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RTS.UI.Control
{
    public class MainCommandScript : MonoBehaviour
    {
        // Import user32.dll to support mouse position reset
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

        private class NavigationUI
        {
            public GameObject navigationUIBase = null;
            public GameObject navigationUISelfCircle = null;
            public GameObject navigationUIHorizontalCircle = null;
            public GameObject navigationUIVerticalCircle = null;
            public GameObject navigationUILine = null;

            public NavigationUI(GameObject navigationUIBasePrefab, GameObject navigationUICirclePrefab, GameObject navigationUILinePrefab)
            {
                navigationUIBase = Instantiate(navigationUIBasePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

                navigationUISelfCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                navigationUIHorizontalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                navigationUIVerticalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

                navigationUILine = Instantiate(navigationUILinePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);

                Cursor.visible = false;
            }

            public void Update(float destinationHorizontalDistance, Vector3 destinationHorizontalPosition, float destinationVerticalDistance)
            {
                Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();

                if (center.x == Mathf.Infinity)
                {
                    Destroy();
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

            public void Destroy()
            {
                if (navigationUIBase != null)
                {
                    GameObject.Destroy(navigationUIBase);
                }
                if (navigationUISelfCircle != null)
                {
                    GameObject.Destroy(navigationUISelfCircle);
                }
                if (navigationUIHorizontalCircle != null)
                {
                    GameObject.Destroy(navigationUIHorizontalCircle);
                }
                if (navigationUIVerticalCircle != null)
                {
                    GameObject.Destroy(navigationUIVerticalCircle);
                }
                if (navigationUILine != null)
                {
                    GameObject.Destroy(navigationUILine);
                }
                Cursor.visible = true;
            }
        }

        private class TargetDisplayUI
        {
            public GameObject from;
            public GameObject circle;
            public GameObject line;

            public virtual bool Update()
            {
                return false;
            }

            public virtual void Destroy()
            {
                
            }
        }

        private class MoveUI : TargetDisplayUI
        {
            public Vector3 to;

            public MoveUI(GameObject from, Vector3 to, GameObject circlePrefab, GameObject linePrefab)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= from.GetComponent<Collider>().bounds.size.magnitude;
                circle.GetComponent<SpriteRenderer>().color = Color.green;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderer.enabled = true;
            }

            // Return false if any of from or to is destoryed
            public override bool Update()
            {
                if (from != null && to != null)
                {
                    circle.transform.position = to;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, from.transform.position);
                    lineRenderer.SetPosition(1, to);
                    return true;
                }
                else
                {
                    Destroy();
                    return false;
                }
            }

            public override void Destroy()
            {
                GameObject.Destroy(circle);
                GameObject.Destroy(line);
            }
        }

        private class FollowUI : TargetDisplayUI
        {
            public GameObject to;

            public FollowUI(GameObject from, GameObject to, GameObject circlePrefab, GameObject linePrefab)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= to.GetComponent<Collider>().bounds.size.magnitude;
                circle.GetComponent<SpriteRenderer>().color = Color.green;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderer.enabled = true;
            }

            // Return false if any of from or to is destoryed
            public override bool Update()
            {
                if (from != null && to != null)
                {
                    circle.transform.position = to.transform.position;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, from.transform.position);
                    lineRenderer.SetPosition(1, to.transform.position);
                    return true;
                }
                else
                {
                    Destroy();
                    return false;
                }
            }

            public override void Destroy()
            {
                GameObject.Destroy(circle);
                GameObject.Destroy(line);
            }
        }

        private class AttackUI : TargetDisplayUI
        {
            public GameObject to;

            public AttackUI(GameObject from, GameObject to, GameObject circlePrefab, GameObject linePrefab)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= to.GetComponent<Collider>().bounds.size.magnitude;
                circle.GetComponent<SpriteRenderer>().color = Color.red;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                lineRenderer.enabled = true;
            }

            // Return false if any of from or to is destoryed
            public override bool Update()
            {
                if (from != null && to != null)
                {
                    circle.transform.position = to.transform.position;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, from.transform.position);
                    lineRenderer.SetPosition(1, to.transform.position);
                    return true;
                }
                else
                {
                    Destroy();
                    return false;
                }
            }

            public override void Destroy()
            {
                GameObject.Destroy(circle);
                GameObject.Destroy(line);
            }
        }

        public float verticalSensitive;
        public float displayTime;

        public GameObject targetDisplayUICirclePrefab;
        public GameObject targetDisplayUILinePrefab;
        public GameObject navigationUIBasePrefab;
        public GameObject navigationUICirclePrefab;
        public GameObject navigationUILinePrefab;

        private NavigationUI navigationUI;
        private List<TargetDisplayUI> allTargetDisplayUI = new List<TargetDisplayUI>();

        private float destinationHorizontalDistance = 0;
        private float destinationVerticalDistance = 0;
        private Vector3 destinationHorizontalPosition = new Vector3();

        private int mousePositionX = 0;
        private int mousePositionY = 0;

        // Update is called once per frame
        void Update()
        {
            if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                // TODO: Move this to another script
                if (Input.GetKeyDown(InputManager.HotKeys.Stop))
                {
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            i.GetComponent<UnitBaseScript>().Stop();
                        }
                    }
                    return;
                }

                if (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction)
                {
                    if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                    {
                        switch (InputManager.InputManagerInstance.CurrentMousePosition)
                        {
                            case InputManager.MousePosition.None:
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.MainCommandMove;
                                break;
                            case InputManager.MousePosition.SelfUnit:
                            case InputManager.MousePosition.FriendUnit:
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.MainCommandFollow;
                                break;
                            case InputManager.MousePosition.EnemyUnit:
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.MainCommandAttack;
                                break;
                        }
                    }
                }

                switch (InputManager.InputManagerInstance.CurrentCommandActionState)
                {
                    case InputManager.CommandActionState.MainCommandMove:
                        // This must be run first, or at the init time, set destination will also be called due to same keydown
                        if (navigationUI != null)
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
                            if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                            {
                                List<UnitBaseScript> allAgents = new List<UnitBaseScript>();
                                foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                                {
                                    if (i.GetComponent<UnitBaseScript>() != null)
                                    {
                                        allAgents.Add(i.GetComponent<UnitBaseScript>());
                                    }
                                }
                                ClearAllTargetDisplayUI();
                                Vector3 destination = destinationHorizontalPosition + new Vector3(0, destinationVerticalDistance, 0);
                                foreach (KeyValuePair<UnitBaseScript, Vector3> i in FindDestination(allAgents, destination, destination -
                                    SelectControlScript.SelectionControlInstance.FindCenter()))
                                {
                                    i.Key.Move(i.Value);
                                    CreateMoveUI(i.Key.gameObject, i.Value);
                                }
                                navigationUI.Destroy();
                                navigationUI = null;
                                StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                            }
                            else
                            {
                                navigationUI.Update(destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
                            }
                        }
                        // If not exist, create, this is the first time of move control, so test where the cursor is and if it is able to do such action
                        else
                        {
                            navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab);
                        }
                        break;
                    case InputManager.CommandActionState.MainCommandFollow:
                        RTSGameObjectBaseScript followTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                        if (followTarget != null && followTarget.BelongTo == GameManager.GameManagerInstance.selfIndex)
                        {
                            ClearAllTargetDisplayUI();
                            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                            {
                                if (i.GetComponent<UnitBaseScript>() != null)
                                {
                                    i.GetComponent<UnitBaseScript>().Follow(followTarget.gameObject);
                                    CreateFollowUI(i, followTarget.gameObject);
                                }
                                StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                            }
                        }
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                        break;
                    case InputManager.CommandActionState.MainCommandAttack:
                        RTSGameObjectBaseScript attackTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                        if (attackTarget != null && attackTarget.BelongTo != GameManager.GameManagerInstance.selfIndex)
                        {
                            ClearAllTargetDisplayUI();
                            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                            {
                                if (i.GetComponent<UnitBaseScript>() != null)
                                {
                                    i.GetComponent<UnitBaseScript>().Attack(attackTarget.gameObject);
                                    CreateAttackUI(i, attackTarget.gameObject);
                                }
                            }
                            StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                        }
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                        break;
                }
                UpdateTargetDisplayUI();
            }
            else
            {
                if (navigationUI != null)
                {
                    navigationUI.Destroy();
                    navigationUI = null;
                }
                ClearAllTargetDisplayUI();
            }
        }

        private IEnumerator ClearTargetDisplayUIWithWaitTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ClearAllTargetDisplayUI();
        }

        private void CreateMoveUI(GameObject from, Vector3 to)
        {
            allTargetDisplayUI.Add(new MoveUI(from, to, targetDisplayUICirclePrefab, targetDisplayUILinePrefab));
        }

        private void CreateFollowUI(GameObject from, GameObject to)
        {
            allTargetDisplayUI.Add(new FollowUI(from, to, targetDisplayUICirclePrefab, targetDisplayUILinePrefab));
        }

        private void CreateAttackUI(GameObject from, GameObject to)
        {
            allTargetDisplayUI.Add(new AttackUI(from, to, targetDisplayUICirclePrefab, targetDisplayUILinePrefab));
        }

        private void UpdateTargetDisplayUI()
        {
            List<TargetDisplayUI> toRemove = new List<TargetDisplayUI>();
            foreach (TargetDisplayUI i in allTargetDisplayUI)
            {
                if (!i.Update())
                {
                    toRemove.Add(i);
                }
            }
            allTargetDisplayUI.RemoveAll(x => toRemove.Contains(x));
        }
        private void ClearAllTargetDisplayUI()
        {
            StopAllCoroutines();
            foreach (TargetDisplayUI i in allTargetDisplayUI)
            {
                i.Destroy();
            }
            allTargetDisplayUI.Clear();
        }

        private Dictionary<UnitBaseScript, Vector3> FindDestination(List<UnitBaseScript> allAgents, Vector3 destination, Vector3 forwardDirection)
        {
            Dictionary<UnitBaseScript, Vector3> temp = new Dictionary<UnitBaseScript, Vector3>();
            Vector3 destinationForEachClass = destination;
            Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;

            List<UnitBaseScript> allFighters = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Fighter);
            if (allFighters.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 10;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFighters = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (UnitBaseScript i in allFighters)
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
                foreach (UnitBaseScript i in allFighters)
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

            List<UnitBaseScript> allFrigates = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Frigate);
            if (allFrigates.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 30;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFrigates = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (UnitBaseScript i in allFrigates)
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
                foreach (UnitBaseScript i in allFrigates)
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

            List<UnitBaseScript> allCruisers = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Cruiser);
            if (allCruisers.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 50;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForCruisers = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (UnitBaseScript i in allCruisers)
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
                foreach (UnitBaseScript i in allCruisers)
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

            List<UnitBaseScript> allBattleships = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Battleship);
            if (allBattleships.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 80;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForBattleships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (UnitBaseScript i in allBattleships)
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
                foreach (UnitBaseScript i in allBattleships)
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

            List<UnitBaseScript> allMotherships = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Mothership);
            if (allMotherships.Count != 0)
            {
                if (destinationForEachClass != destination)
                {
                    destinationForEachClass -= forwardDirection.normalized * 120;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForMotherships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                foreach (UnitBaseScript i in allMotherships)
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
                foreach (UnitBaseScript i in allMotherships)
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
    }
}
