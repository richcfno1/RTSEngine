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
                    else if (Input.GetKeyDown(InputManager.HotKeys.Follow))
                    {
                        OnFollowButtonClicked();
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.LookAt))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtWaitingNextKey;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Rotate))
                    {
                        OnRotateButtonClicked();
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
                        else if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnAttackMovingButtonClicked();
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
                                navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab, Color.red);
                            }
                        }
                        break;
                    case InputManager.CommandActionState.LookAtWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnLookAtTargetButtonClicked();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnLookAtSpaceButtonClicked();
                        }
                        break;
                    case InputManager.CommandActionState.AttackTarget:
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
                        break;
                    case InputManager.CommandActionState.AttackMoving:
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
                            navigationUI = new NavigationUI(navigationUIBasePrefab, navigationUICirclePrefab, navigationUILinePrefab, Color.red);
                        }
                        break;
                    default:
                        Debug.Log("Cancel " + InputManager.InputManagerInstance.CurrentCommandActionState);
                        //InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
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
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Follow;
        }

        public void OnLookAtTargetButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtTarget;
        }

        public void OnLookAtSpaceButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtSpace;
        }

        public void OnRotateButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Rotate;
        }
    }
}