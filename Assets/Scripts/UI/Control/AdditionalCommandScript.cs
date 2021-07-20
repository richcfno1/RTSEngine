using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI.Control
{
    public class AdditionalCommandScript : CommandBaseScript
    {
        public List<Button> buttons;
        // Update is called once per frame
        void Update()
        {
            if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                if (SelectControlScript.SelectionControlInstance.SelectedChanged)
                {
                    InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                    if (navigationUI != null)
                    {
                        navigationUI.Destroy();
                        navigationUI = null;
                    }
                    ClearAllTargetDisplayUI();
                }

                foreach (Button i in buttons)
                {
                    i.interactable = true;
                }
                // Stop can be called in any case
                if (Input.GetKeyDown(InputManager.HotKeys.Stop))
                {
                    OnStopButtonClicked();
                }
                if (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction &&
                    InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.NoAction)
                {
                    if (Input.GetKeyDown(InputManager.HotKeys.Attack))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.AttackWaitingNextKey;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.LookAt))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtWaitingNextKey;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Follow))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.FollowWaitingNextKey;
                    }
                }
                switch (InputManager.InputManagerInstance.CurrentCommandActionState)
                {
                    case InputManager.CommandActionState.Stop:
                        foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                        {
                            if (i.GetComponent<UnitBaseScript>() != null)
                            {
                                i.GetComponent<UnitBaseScript>().Stop();
                            }
                        }
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                        break;
                    case InputManager.CommandActionState.AttackWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnAttackTargetButtonClicked();
                            AttackTarget();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnAttackMovingButtonClicked();
                            AttackMove();
                        }
                        break;
                    case InputManager.CommandActionState.FollowWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnFollowButtonClicked();
                            Follow();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnMoveButtonClicked();
                            Move();
                        }
                        break;
                    case InputManager.CommandActionState.LookAtWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnLookAtTargetButtonClicked();
                            LookAtTarget();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnLookAtSpaceButtonClicked();
                            LookAtSpace();
                        }
                        break;
                    case InputManager.CommandActionState.AttackTarget:
                        AttackTarget();
                        break;
                    case InputManager.CommandActionState.AttackMoving:
                        AttackMove();
                        break;
                    case InputManager.CommandActionState.FollowTarget:
                        Follow();
                        break;
                    case InputManager.CommandActionState.Move:
                        Move();
                        break;
                    case InputManager.CommandActionState.LookAtTarget:
                        LookAtTarget();
                        break;
                    case InputManager.CommandActionState.LookAtSpace:
                        LookAtSpace();
                        break;
                }
                UpdateTargetDisplayUI();
            }
            else
            {
                foreach (Button i in buttons)
                {
                    i.interactable = false;
                }
                if (navigationUI != null)
                {
                    navigationUI.Destroy();
                    navigationUI = null;
                }
                ClearAllTargetDisplayUI();
            }
        }

        private void AttackTarget()
        {
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript attackTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (attackTarget != null && attackTarget.BelongTo != GameManager.GameManagerInstance.selfIndex)
                {
                    ClearAllTargetDisplayUI();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            i.GetComponent<UnitBaseScript>().Attack(attackTarget.gameObject);
                            CreateGOToGOUI(i, attackTarget.gameObject, Color.red);
                        }
                    }
                    StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
            }
        }

        private void AttackMove()
        {
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
                    Plane hPlane = new Plane(Vector3.Cross(center - destinationHorizontalPosition, Vector3.up), destinationHorizontalPosition);
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
                        i.Key.AttackAndMove(i.Value);
                        CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.red);
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
                destinationHorizontalDistance = 0;
                destinationVerticalDistance = 0;
                destinationHorizontalPosition = new Vector3();
                navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab, Color.red);
            }
        }

        private void Follow()
        {
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript followTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (followTarget != null)
                {
                    ClearAllTargetDisplayUI();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            i.GetComponent<UnitBaseScript>().Follow(followTarget.gameObject);
                            CreateGOToGOUI(i, followTarget.gameObject, Color.green);
                        }
                        StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
            }
        }

        private void Move()
        {
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
                    Plane hPlane = new Plane(Vector3.Cross(center - destinationHorizontalPosition, Vector3.up), destinationHorizontalPosition);
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
                        CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.green);
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
                destinationHorizontalDistance = 0;
                destinationVerticalDistance = 0;
                destinationHorizontalPosition = new Vector3();
                navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab, Color.green);
            }
        }

        private void LookAtTarget()
        {
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript followTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (followTarget != null)
                {
                    ClearAllTargetDisplayUI();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            i.GetComponent<UnitBaseScript>().LookAtTarget(followTarget.gameObject);
                            CreateGOToGOUI(i, followTarget.gameObject, Color.yellow);
                        }
                        StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
            }
        }

        private void LookAtSpace()
        {
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
                    Plane hPlane = new Plane(Vector3.Cross(center - destinationHorizontalPosition, Vector3.up), destinationHorizontalPosition);
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
                        i.Key.LookAt(i.Value);
                        CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.yellow);
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
                destinationHorizontalDistance = 0;
                destinationVerticalDistance = 0;
                destinationHorizontalPosition = new Vector3();
                navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab, Color.yellow);
            }
        }

        public void OnStopButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Stop;
        }

        public void OnAttackTargetButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.AttackTarget;
        }

        public void OnAttackMovingButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.AttackMoving;
        }

        public void OnFollowButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.FollowTarget;
        }

        public void OnMoveButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Move;
        }

        public void OnLookAtTargetButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtTarget;
        }

        public void OnLookAtSpaceButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtSpace;
        }
    }
}