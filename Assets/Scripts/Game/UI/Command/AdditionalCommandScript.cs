using RTS.Game.Ability.SpecialAbility;
using RTS.Game.RTSGameObject;
using RTS.Game.RTSGameObject.Unit;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.Game.UI.Command
{
    public class AdditionalCommandScript : CommandBaseScript
    {
        [Header("Additional Command")]
        public List<Button> buttons;
        public List<Button> skillButtons;  // size shoule bd 5 with order E R D F C

        [Header("Fire Control State")]
        public Button aggressiveButton;
        public Image aggressiveButtonEnabledCover;
        public Button neutralButton;
        public Image neutralButtonEnabledCover;
        public Button passiveButton;
        public Image passiveButtonEnabledCover;

        private SortedDictionary<string, List<SpecialAbilityBaseScript>> showingAbilities = new SortedDictionary<string, List<SpecialAbilityBaseScript>>();

        // Update is called once per frame
        void Update()
        {
            // draw skill icons
            showingAbilities = SelectControlScript.SelectionControlInstance.GetAllSpecialAbilityOfMainSelected();
            int indexOfSkillButtons = 0;
            foreach (KeyValuePair<string, List<SpecialAbilityBaseScript>> i in showingAbilities)
            {
                if (indexOfSkillButtons >= skillButtons.Count)
                {
                    Debug.LogWarning($"Number of special ability {indexOfSkillButtons + 1} is more than number of skill buttons {skillButtons.Count}");
                    break;
                }
                skillButtons[indexOfSkillButtons].gameObject.SetActive(true);
                if (i.Value.Count == 0)
                {
                    Debug.LogError($"Impossible case: there is a special ability key {i.Key} without any instance of it");
                }
                else
                {
                    skillButtons[indexOfSkillButtons].GetComponent<Image>().sprite = i.Value.FirstOrDefault().specialAbilityIcon;
                    float maxCoolDown = 0;
                    foreach (SpecialAbilityBaseScript j in i.Value)
                    {
                        maxCoolDown = maxCoolDown > j.GetCoolDownPercent() ? maxCoolDown : j.GetCoolDownPercent();
                    }
                    skillButtons[indexOfSkillButtons].GetComponentsInChildren<Image>()[1].fillAmount = 1 - maxCoolDown;
                }
                indexOfSkillButtons++;
            }
            for (int i = indexOfSkillButtons; i < skillButtons.Count; i++)
            {
                skillButtons[i].gameObject.SetActive(false);
            }
            // At this point, indexOfSkillButtons will be the amount of valid skill button.

            // Update firecontrol showing
            if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                aggressiveButton.interactable = true;
                aggressiveButtonEnabledCover.enabled = false;
                neutralButton.interactable = true;
                neutralButtonEnabledCover.enabled = false;
                passiveButton.interactable = true;
                passiveButtonEnabledCover.enabled = false;
                foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                {
                    if (i != null && i.GetComponent<UnitBaseScript>() != null)
                    {
                        switch (i.GetComponent<UnitBaseScript>().CurrentFireControlStatus)
                        {
                            case UnitBaseScript.FireControlStatus.Aggressive:
                                aggressiveButtonEnabledCover.enabled = true;
                                break;
                            case UnitBaseScript.FireControlStatus.Neutral:
                                neutralButtonEnabledCover.enabled = true;
                                break;
                            case UnitBaseScript.FireControlStatus.Passive:
                                passiveButtonEnabledCover.enabled = true;
                                break;
                        }
                    }
                }
            }
            else
            {
                aggressiveButton.interactable = false;
                aggressiveButtonEnabledCover.enabled = false;
                neutralButton.interactable = false;
                neutralButtonEnabledCover.enabled = false;
                passiveButton.interactable = false;
                passiveButtonEnabledCover.enabled = false;
            }

            // Command
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
                foreach (Button i in skillButtons)
                {
                    i.interactable = true;
                }
                // Stop can be called in any case
                if (Input.GetKeyDown(InputManager.HotKeys.Stop))
                {
                    OnStopButtonClicked();
                }
                if (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction &&
                    InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.NoAction &&
                    !Input.GetKey(InputManager.HotKeys.FireControlKey))
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
                    else if (Input.GetKeyDown(InputManager.HotKeys.Skill1))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill1;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Skill2))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill2;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Skill3))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill3;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Skill4))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill4;
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Skill5))
                    {
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill5;
                    }
                }
                else if (Input.GetKey(InputManager.HotKeys.FireControlKey))
                {
                    if (Input.GetKeyDown(InputManager.HotKeys.Aggressive))
                    {
                        SetAggressive();
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Neutral))
                    {
                        SetNeutral();
                    }
                    else if (Input.GetKeyDown(InputManager.HotKeys.Passive))
                    {
                        SetPassive();
                    }
                }
                switch (InputManager.InputManagerInstance.CurrentCommandActionState)
                {
                    case InputManager.CommandActionState.Stop:
                        foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                        {
                            if (i.GetComponent<UnitBaseScript>() != null)
                            {
                                i.GetComponent<UnitBaseScript>().Stop();
                            }
                        }
                        ClearAllTargetDisplayUI();
                        if (navigationUI != null)
                        {
                            navigationUI.Destroy();
                            navigationUI = null;
                        }
                        GetComponent<MainCommandScript>().ClearAllUIWhenStop();
                        InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                        break;
                    case InputManager.CommandActionState.AttackWaitingNextKey:
                        switch (InputManager.InputManagerInstance.CurrentMousePosition)
                        {
                            case InputManager.MousePosition.None:
                            case InputManager.MousePosition.SelfUnit:
                            case InputManager.MousePosition.FriendUnit:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                                break;
                            case InputManager.MousePosition.NeutrualUnit:
                            case InputManager.MousePosition.EnemyUnit:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                                break;
                        }
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
                        switch (InputManager.InputManagerInstance.CurrentMousePosition)
                        {
                            case InputManager.MousePosition.None:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                                break;
                            case InputManager.MousePosition.SelfUnit:
                            case InputManager.MousePosition.FriendUnit:
                            case InputManager.MousePosition.NeutrualUnit:
                            case InputManager.MousePosition.EnemyUnit:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                                break;
                        }
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
                        switch (InputManager.InputManagerInstance.CurrentMousePosition)
                        {
                            case InputManager.MousePosition.None:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                                break;
                            case InputManager.MousePosition.SelfUnit:
                            case InputManager.MousePosition.FriendUnit:
                            case InputManager.MousePosition.NeutrualUnit:
                            case InputManager.MousePosition.EnemyUnit:
                                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                                break;
                        }
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
                    case InputManager.CommandActionState.Skill1:
                        UseSkill(indexOfSkillButtons, 0);
                        break;
                    case InputManager.CommandActionState.Skill2:
                        UseSkill(indexOfSkillButtons, 1);
                        break;
                    case InputManager.CommandActionState.Skill3:
                        UseSkill(indexOfSkillButtons, 2);
                        break;
                    case InputManager.CommandActionState.Skill4:
                        UseSkill(indexOfSkillButtons, 3);
                        break;
                    case InputManager.CommandActionState.Skill5:
                        UseSkill(indexOfSkillButtons, 4);
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
                foreach (Button i in skillButtons)
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
            switch (InputManager.InputManagerInstance.CurrentMousePosition)
            {
                case InputManager.MousePosition.None:
                case InputManager.MousePosition.SelfUnit:
                case InputManager.MousePosition.FriendUnit:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                    break;
                case InputManager.MousePosition.NeutrualUnit:
                case InputManager.MousePosition.EnemyUnit:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                    break;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript attackTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (attackTarget != null && attackTarget.BelongTo != GameManager.GameManagerInstance.SelfIndex)
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
                    Vector3 destination = attackTarget.transform.position + attackTarget.radius * 2 *
                        (SelectControlScript.SelectionControlInstance.FindCenter() - attackTarget.transform.position).normalized;
                    foreach (KeyValuePair<UnitBaseScript, Vector3> i in FindDestination(allAgents, destination, destination -
                        SelectControlScript.SelectionControlInstance.FindCenter(), true))
                    {
                        if (i.Key != null)
                        {
                            if (i.Key.AttackAbility != null)
                            {
                                i.Key.AttackAbility.Attack(attackTarget.Index);
                                CreateGOToGOUI(i.Key.gameObject, attackTarget.gameObject, Color.red);
                            }
                            else if (i.Key.MoveAbility != null)
                            {
                                i.Key.MoveAbility.Follow(attackTarget.Index, i.Value - attackTarget.transform.position);
                            }
                            CreateGOToGOUI(i.Key.gameObject, attackTarget.gameObject, Color.green);
                        }
                        StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
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
                        if (i.Key.AttackAbility != null && i.Key.MoveAbility != null)
                        {
                            i.Key.AttackAbility.AttackAndMove(i.Value);
                            CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.red);
                        }
                        else if (i.Key.MoveAbility != null)
                        {
                            i.Key.MoveAbility.Move(i.Value);
                            CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.green);
                        }
                    }
                    navigationUI.Destroy();
                    navigationUI = null;
                    StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
                }
                else
                {
                    navigationUI.Update(SelectControlScript.SelectionControlInstance.FindCenter(), 
                        destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
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
            switch (InputManager.InputManagerInstance.CurrentMousePosition)
            {
                case InputManager.MousePosition.None:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                    break;
                case InputManager.MousePosition.SelfUnit:
                case InputManager.MousePosition.FriendUnit:
                case InputManager.MousePosition.NeutrualUnit:
                case InputManager.MousePosition.EnemyUnit:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                    break;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript followTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (followTarget != null)
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
                    Vector3 destination = followTarget.transform.position + followTarget.radius * 2 *
                        (SelectControlScript.SelectionControlInstance.FindCenter() - followTarget.transform.position).normalized;
                    foreach (KeyValuePair<UnitBaseScript, Vector3> i in FindDestination(allAgents, destination, destination -
                        SelectControlScript.SelectionControlInstance.FindCenter(), true))
                    {
                        if (i.Key != null)
                        {
                            if (i.Key.MoveAbility != null)
                            {
                                i.Key.MoveAbility.Follow(followTarget.Index, i.Value - followTarget.transform.position);
                            }
                            CreateGOToGOUI(i.Key.gameObject, followTarget.gameObject, Color.green);
                        }
                        StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
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
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
                }
                else
                {
                    navigationUI.Update(SelectControlScript.SelectionControlInstance.FindCenter(), 
                        destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
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
            switch (InputManager.InputManagerInstance.CurrentMousePosition)
            {
                case InputManager.MousePosition.None:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
                    break;
                case InputManager.MousePosition.SelfUnit:
                case InputManager.MousePosition.FriendUnit:
                case InputManager.MousePosition.NeutrualUnit:
                case InputManager.MousePosition.EnemyUnit:
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
                    break;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                RTSGameObjectBaseScript lookAtTarget = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (lookAtTarget != null)
                {
                    ClearAllTargetDisplayUI();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            if (i.GetComponent<UnitBaseScript>().MoveAbility != null)
                            {
                                i.GetComponent<UnitBaseScript>().MoveAbility.LookAtTarget(lookAtTarget.Index);
                            }
                            CreateGOToGOUI(i, lookAtTarget.gameObject, Color.yellow);
                        }
                        StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
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
                            i.Key.MoveAbility.LookAt(i.Value);
                        }
                        CreateGOToVectorUI(i.Key.gameObject, i.Value, Color.yellow);
                    }
                    navigationUI.Destroy();
                    navigationUI = null;
                    StartCoroutine(ClearTargetDisplayUIWithWaitTime(displayTime));
                    InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
                }
                else
                {
                    navigationUI.Update(SelectControlScript.SelectionControlInstance.FindCenter(), 
                        destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
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

        private void UseSkill(int numberOfSkills, int index)
        {
            if (index >= numberOfSkills)
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                return;
            }
            string skillType = showingAbilities.Keys.ToList()[index];

            if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(NoSelectionSpecialAbilityScript)) != default)
            {
                //Dictionary<UnitBaseScript, List<NoSelectionSpecialAbilityScript>> unitToSpcialAbilityList = new Dictionary<UnitBaseScript, List<NoSelectionSpecialAbilityScript>>();
                //foreach (SpecialAbilityBaseScript i in showingAbilities[skillType])
                //{
                //    if (unitToSpcialAbilityList.ContainsKey(i.Host))
                //    {
                //        unitToSpcialAbilityList[i.Host].Add((NoSelectionSpecialAbilityScript)i);
                //    }
                //    else
                //    {
                //        unitToSpcialAbilityList.Add(i.Host, new List<NoSelectionSpecialAbilityScript>() { (NoSelectionSpecialAbilityScript)i });
                //    }
                //}
                UseNoSelectionSkill(showingAbilities[skillType].Where(x => x.GetType() == typeof(NoSelectionSpecialAbilityScript)).ToList());
            }
            else if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(SelectTargetSpecialAbilityScript)) != default)
            {
                //Dictionary<UnitBaseScript, List<SelectTargetSpecialAbilityScript>> unitToSpcialAbilityList = new Dictionary<UnitBaseScript, List<SelectTargetSpecialAbilityScript>>();
                //foreach (SpecialAbilityBaseScript i in showingAbilities[skillType])
                //{
                //    if (unitToSpcialAbilityList.ContainsKey(i.Host))
                //    {
                //        unitToSpcialAbilityList[i.Host].Add((SelectTargetSpecialAbilityScript)i);
                //    }
                //    else
                //    {
                //        unitToSpcialAbilityList.Add(i.Host, new List<SelectTargetSpecialAbilityScript>() { (SelectTargetSpecialAbilityScript)i });
                //    }
                //}
                UseSelectTargetSkill(showingAbilities[skillType].Where(x => x.GetType() == typeof(SelectTargetSpecialAbilityScript)).ToList());
            }
            else if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(SelectSpaceSpecialAbilityScript)) != default)
            {
                //Dictionary<UnitBaseScript, List<SelectSpaceSpecialAbilityScript>> unitToSpcialAbilityList = new Dictionary<UnitBaseScript, List<SelectSpaceSpecialAbilityScript>>();
                //foreach (SpecialAbilityBaseScript i in showingAbilities[skillType])
                //{
                //    if (unitToSpcialAbilityList.ContainsKey(i.Host))
                //    {
                //        unitToSpcialAbilityList[i.Host].Add((SelectSpaceSpecialAbilityScript)i);
                //    }
                //    else
                //    {
                //        unitToSpcialAbilityList.Add(i.Host, new List<SelectSpaceSpecialAbilityScript>() { (SelectSpaceSpecialAbilityScript)i });
                //    }
                //}
                UseSelectSpaceSkill(showingAbilities[skillType].Where(x => x.GetType() == typeof(SelectSpaceSpecialAbilityScript)).ToList());
            }
            else
            {
                Debug.LogWarning($"Wrong type of special ability script: {showingAbilities[skillType].FirstOrDefault().GetType()}");
            }
        }

        private void UseNoSelectionSkill(List<SpecialAbilityBaseScript> abilities)
        {
            foreach (SpecialAbilityBaseScript i in abilities)
            {
                NoSelectionSpecialAbilityScript temp = (NoSelectionSpecialAbilityScript)i;
                temp.UseAbility();
            }
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
            InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
        }

        private void UseSelectTargetSkill(List<SpecialAbilityBaseScript> abilities)
        {
            InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Command;
            RTSGameObjectBaseScript target = InputManager.InputManagerInstance.PointedRTSGameObject;
            bool valid = false;
            if (target != null && ((SelectTargetSpecialAbilityScript)abilities.FirstOrDefault()).possibleTargetTags.Contains(target.tag) &&
                ((SelectTargetSpecialAbilityScript)abilities.FirstOrDefault()).possibleRelations.Contains(
                GameManager.GameManagerInstance.GetPlayerRelation(abilities.FirstOrDefault().Host.BelongTo, 
                target.GetComponent<RTSGameObjectBaseScript>().BelongTo)))
            {
                valid = true;
                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.ValidTarget;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.SelectTarget))
            {
                if (valid)
                {
                    foreach (SpecialAbilityBaseScript i in abilities)
                    {
                        SelectTargetSpecialAbilityScript temp = (SelectTargetSpecialAbilityScript)i;
                        temp.UseAbility(target.Index);
                    }
                }
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
            }
        }

        private void UseSelectSpaceSkill(List<SpecialAbilityBaseScript> abilities)
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

                    foreach (SpecialAbilityBaseScript i in abilities)
                    {
                        SelectSpaceSpecialAbilityScript temp = (SelectSpaceSpecialAbilityScript)i;
                        temp.UseAbility(destination);
                    }
                    navigationUI.Destroy();
                    navigationUI = null;
                    InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                    InputManager.InputManagerInstance.CurrentMouseTexture = InputManager.MouseTexture.Normal;
                }
                else
                {
                    Vector3 center = new Vector3();
                    foreach (SpecialAbilityBaseScript i in abilities)
                    {
                        center += i.Host.transform.position;
                    }
                    center /= abilities.Count;
                    navigationUI.Update(center, destinationHorizontalDistance, destinationHorizontalPosition, destinationVerticalDistance);
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

        public void SetAggressive()
        {
            if (!SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                return;
            }
            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
            {
                if (i.GetComponent<UnitBaseScript>() != null && i.GetComponent<UnitBaseScript>().AttackAbility != null)
                {
                    i.GetComponent<UnitBaseScript>().AttackAbility.SetAggressive();
                }
            }
        }

        public void SetNeutral()
        {
            if (!SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                return;
            }
            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
            {
                if (i.GetComponent<UnitBaseScript>() != null && i.GetComponent<UnitBaseScript>().AttackAbility != null)
                {
                    i.GetComponent<UnitBaseScript>().AttackAbility.SetNeutral();
                }
            }
        }

        public void SetPassive()
        {
            if (!SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                return;
            }
            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
            {
                if (i.GetComponent<UnitBaseScript>() != null && i.GetComponent<UnitBaseScript>().AttackAbility != null)
                {
                    i.GetComponent<UnitBaseScript>().AttackAbility.SetPassive();
                }
            }
        }

        public void OnStopButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Stop;
            }
        }

        public void OnAttackTargetButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.AttackTarget;
            }
        }

        public void OnAttackMovingButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.AttackMoving;
            }
        }

        public void OnFollowButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.FollowTarget;
            }
        }

        public void OnMoveButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Move;
            }
        }

        public void OnLookAtTargetButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtTarget;
            }
        }

        public void OnLookAtSpaceButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.LookAtSpace;
            }
        }

        public void OnSkill1ButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill1;
            }
        }

        public void OnSkill2ButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill2;
            }
        }

        public void OnSkill3ButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill3;
            }
        }

        public void OnSkill4ButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill4;
            }
        }

        public void OnSkill5ButtonClicked()
        {
            if (!Input.GetKey(InputManager.HotKeys.FireControlKey))
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill5;
            }
        }
    }
}