using RTS.Game.RTSGameObject;
using RTS.Game.RTSGameObject.Unit;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RTS.Game.UI.Command
{
    public class CommandBaseScript : MonoBehaviour
    {
        // Import user32.dll to support mouse position reset
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        protected static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        protected static extern bool SetCursorPos(int X, int Y);

        protected class NavigationUI
        {
            public GameObject navigationUIBase = null;
            public GameObject navigationUISelfCircle = null;
            public GameObject navigationUIHorizontalCircle = null;
            public GameObject navigationUIVerticalCircle = null;
            public GameObject navigationUILine = null;

            public NavigationUI(GameObject navigationUIBasePrefab, GameObject navigationUICirclePrefab, GameObject navigationUILinePrefab, Color color)
            {
                navigationUIBase = Instantiate(navigationUIBasePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

                navigationUISelfCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                navigationUIHorizontalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                navigationUIVerticalCircle = Instantiate(navigationUICirclePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);

                navigationUILine = Instantiate(navigationUILinePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);

                LineRenderer line = navigationUILine.GetComponent<LineRenderer>();
                line.positionCount = 4;
                line.startColor = color;
                line.endColor = color;
                line.enabled = true;

                Cursor.visible = false;
            }

            public void Update(Vector3 center, float destinationHorizontalDistance, Vector3 destinationHorizontalPosition, float destinationVerticalDistance)
            {
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

        protected class TargetDisplayUI
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

        protected class GOToVectorUI : TargetDisplayUI
        {
            public Vector3 to;

            public GOToVectorUI(GameObject from, Vector3 to, GameObject circlePrefab, GameObject linePrefab, Color color)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= from.GetComponent<Collider>().bounds.size.magnitude;
                circle.GetComponent<SpriteRenderer>().color = color;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
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

        protected class GOToGOUI : TargetDisplayUI
        {
            public GameObject to;

            public GOToGOUI(GameObject from, GameObject to, GameObject circlePrefab, GameObject linePrefab, Color color)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= to.GetComponent<Collider>().bounds.size.magnitude;
                circle.GetComponent<SpriteRenderer>().color = color;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
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

        protected NavigationUI navigationUI;
        protected List<TargetDisplayUI> allTargetDisplayUI = new List<TargetDisplayUI>();

        protected float destinationHorizontalDistance = 0;
        protected float destinationVerticalDistance = 0;
        protected Vector3 destinationHorizontalPosition = new Vector3();

        // For user32.dll
        protected int mousePositionX = 0;
        protected int mousePositionY = 0;

        protected IEnumerator ClearTargetDisplayUIWithWaitTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ClearAllTargetDisplayUI();
        }

        protected void CreateGOToVectorUI(GameObject from, Vector3 to, Color color)
        {
            allTargetDisplayUI.Add(new GOToVectorUI(from, to, targetDisplayUICirclePrefab, targetDisplayUILinePrefab, color));
        }

        protected void CreateGOToGOUI(GameObject from, GameObject to, Color color)
        {
            allTargetDisplayUI.Add(new GOToGOUI(from, to, targetDisplayUICirclePrefab, targetDisplayUILinePrefab, color));
        }

        protected void UpdateTargetDisplayUI()
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
        protected void ClearAllTargetDisplayUI()
        {
            StopAllCoroutines();
            foreach (TargetDisplayUI i in allTargetDisplayUI)
            {
                i.Destroy();
            }
            allTargetDisplayUI.Clear();
        }

        protected Dictionary<UnitBaseScript, Vector3> FindDestination(List<UnitBaseScript> allAgents, Vector3 destination, Vector3 forwardDirection, bool addOffsetToFirstLine = false)
        {
            Dictionary<UnitBaseScript, Vector3> temp = new Dictionary<UnitBaseScript, Vector3>();
            Vector3 destinationForEachClass = destination;
            Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;

            List<UnitBaseScript> allFighters = allAgents.FindAll(x => x.objectScale == RTSGameObjectBaseScript.ObjectScale.Fighter);
            if (allFighters.Count != 0)
            {
                if (destinationForEachClass != destination || addOffsetToFirstLine)
                {
                    destinationForEachClass -= forwardDirection.normalized * 10;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFighters = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                int maxNumberInSameLine = 15;
                int lineCounter = 0;
                int unitGapDistance = 20;
                for (int i = 0; i < allFighters.Count; i++)
                {
                    int lineIndex = i % maxNumberInSameLine;
                    destinationForFighters.Add(tempDestination + 
                        (lineIndex % 2 == 0 ? -1 : 1) * right * unitGapDistance * ((lineIndex + 1) / 2));
                    lineCounter++;
                    if (lineCounter == maxNumberInSameLine)
                    {
                        lineCounter = 0;
                        tempDestination += Vector3.up * unitGapDistance;
                    }
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
                if (destinationForEachClass != destination || addOffsetToFirstLine)
                {
                    destinationForEachClass -= forwardDirection.normalized * 30;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForFrigates = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                int maxNumberInSameLine = 5;
                int lineCounter = 0;
                int unitGapDistance = 60;
                for (int i = 0; i < allFrigates.Count; i++)
                {
                    int lineIndex = i % maxNumberInSameLine;
                    destinationForFrigates.Add(tempDestination +
                        (lineIndex % 2 == 0 ? -1 : 1) * right * unitGapDistance * ((lineIndex + 1) / 2));
                    lineCounter++;
                    if (lineCounter == maxNumberInSameLine)
                    {
                        lineCounter = 0;
                        tempDestination += Vector3.up * unitGapDistance;
                    }
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
                if (destinationForEachClass != destination || addOffsetToFirstLine)
                {
                    destinationForEachClass -= forwardDirection.normalized * 50;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForCruisers = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                int maxNumberInSameLine = 3;
                int lineCounter = 0;
                int unitGapDistance = 100;
                for (int i = 0; i < allCruisers.Count; i++)
                {
                    int lineIndex = i % maxNumberInSameLine;
                    destinationForCruisers.Add(tempDestination +
                        (lineIndex % 2 == 0 ? -1 : 1) * right * unitGapDistance * ((lineIndex + 1) / 2));
                    lineCounter++;
                    if (lineCounter == maxNumberInSameLine)
                    {
                        lineCounter = 0;
                        tempDestination += Vector3.up * unitGapDistance;
                    }
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
                if (destinationForEachClass != destination || addOffsetToFirstLine)
                {
                    destinationForEachClass -= forwardDirection.normalized * 80;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForBattleships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                int maxNumberInSameLine = 3;
                int lineCounter = 0;
                int unitGapDistance = 160;
                for (int i = 0; i < allBattleships.Count; i++)
                {
                    int lineIndex = i % maxNumberInSameLine;
                    destinationForBattleships.Add(tempDestination +
                        (lineIndex % 2 == 0 ? -1 : 1) * right * unitGapDistance * ((lineIndex + 1) / 2));
                    lineCounter++;
                    if (lineCounter == maxNumberInSameLine)
                    {
                        lineCounter = 0;
                        tempDestination += Vector3.up * unitGapDistance;
                    }
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
                if (destinationForEachClass != destination || addOffsetToFirstLine)
                {
                    destinationForEachClass -= forwardDirection.normalized * 120;
                }

                // Find a list of detination where perpendicular to "forward direction", then adjust its center to destinationForEachClass
                List<Vector3> destinationForMotherships = new List<Vector3>();
                Vector3 tempDestination = destinationForEachClass;
                int maxNumberInSameLine = 2;
                int lineCounter = 0;
                int unitGapDistance = 240;
                for (int i = 0; i < allMotherships.Count; i++)
                {
                    int lineIndex = i % maxNumberInSameLine;
                    destinationForMotherships.Add(tempDestination +
                        (lineIndex % 2 == 0 ? -1 : 1) * right * unitGapDistance * ((lineIndex + 1) / 2));
                    lineCounter++;
                    if (lineCounter == maxNumberInSameLine)
                    {
                        lineCounter = 0;
                        tempDestination += Vector3.up * unitGapDistance;
                    }
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
