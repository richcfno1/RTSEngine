using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.UI.Command
{
    public class MainCommandScript : CommandBaseScript
    {
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

                if (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction &&
                    InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.NoAction)
                {
                    // Note: current we only have 3 cursor texture, and adding valid texture for main command 
                    //       may make player confusing, especially when they press A and LMB to attack target, 
                    //       then command state back to main command, and cursor is still valid texture. 
                    //       However, at this point player need RMB to command another attack. THIS IS BAD.
                    //switch (InputManager.InputManagerInstance.CurrentMousePosition)
                    //{
                    //    case InputManager.MousePosition.None:
                    //        InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
                    //        break;
                    //    case InputManager.MousePosition.SelfUnit:
                    //    case InputManager.MousePosition.FriendUnit:
                    //    case InputManager.MousePosition.EnemyUnit:
                    //        InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                    //        break;
                    //}

                    if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                    {
                        switch (InputManager.InputManagerInstance.CurrentMousePosition)
                        {
                            case InputManager.MousePosition.None:
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.MainCommandMove;
                                break;
                            case InputManager.MousePosition.SelfUnit:
                            case InputManager.MousePosition.FriendUnit:
                            case InputManager.MousePosition.NeutrualUnit:
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
                                Plane hPlane = new Plane(Camera.main.transform.forward, destinationHorizontalPosition);
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
                                foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
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
                                    if (i.Key.MoveAbility != null)
                                    {
                                        i.Key.MoveAbility.Move(i.Value);
                                    }
                                    CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.green);
                                }
                                navigationUI.Destroy();
                                navigationUI = null;
                                StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                            }
                            else
                            {
                                navigationUI.Update(SelectControlScript.SelectionControlInstance.FindCenter(), destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
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
                        break;
                    case InputManager.CommandActionState.MainCommandFollow:
                        RTSGameObjectBaseScript followTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                        if (followTarget != null && followTarget.BelongTo == GameManager.GameManagerInstance.selfIndex)
                        {
                            ClearAllTargetDisplayUI();
                            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                            {
                                if (i.GetComponent<UnitBaseScript>() != null)
                                {
                                    if (i.GetComponent<UnitBaseScript>().MoveAbility != null)
                                    {
                                        i.GetComponent<UnitBaseScript>().MoveAbility.Follow(followTarget.gameObject);
                                    }
                                    CreateGOToGOUI(i, followTarget.gameObject, Color.green);
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
                            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                            {
                                if (i.GetComponent<UnitBaseScript>() != null)
                                {
                                    if (i.GetComponent<UnitBaseScript>().AttackAbility != null)
                                    {
                                        i.GetComponent<UnitBaseScript>().AttackAbility.Attack(attackTarget.gameObject);
                                        CreateGOToGOUI(i, attackTarget.gameObject, Color.red);
                                    }
                                    else if (i.GetComponent<UnitBaseScript>().MoveAbility != null)
                                    {
                                        i.GetComponent<UnitBaseScript>().MoveAbility.Follow(attackTarget.gameObject);
                                        CreateGOToGOUI(i, attackTarget.gameObject, Color.green);
                                    }
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

        public void ClearAllUIWhenStop()
        {
            ClearAllTargetDisplayUI();
            if (navigationUI != null)
            {
                navigationUI.Destroy();
                navigationUI = null;
            }
        }
    }
}
