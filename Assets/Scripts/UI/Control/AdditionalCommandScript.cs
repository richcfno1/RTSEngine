using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI.Control
{
    public class AdditionalCommandScript : MonoBehaviour
    {
        public List<Button> buttons;
        // Update is called once per frame
        void Update()
        {
            if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                foreach (Button i in buttons)
                {
                    i.enabled = true;
                }
                // Stop can be called in any case
                if (Input.GetKeyDown(InputManager.HotKeys.Stop))
                {
                    OnStopButtonClicked();
                }
                if (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction)
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
                        return;
                    case InputManager.CommandActionState.AttackWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnAttackTargetButtonClicked();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnAttackTargetButtonClicked();
                        }
                        return;
                    case InputManager.CommandActionState.LookAtWaitingNextKey:
                        if (Input.GetKeyDown(InputManager.HotKeys.MainCommand))
                        {
                            OnLookAtSpaceButtonClicked();
                        }
                        else if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
                        {
                            OnLookAtTargetButtonClicked();
                        }
                        return;
                    default:
                        Debug.Log("Cancel " + InputManager.InputManagerInstance.CurrentCommandActionState);
                        //InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                        return;
                }
            }
            else
            {
                foreach (Button i in buttons)
                {
                    i.enabled = false;
                }
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