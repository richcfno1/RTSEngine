using RTS.Ability.SpecialAbility;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI.Command
{
    public class AdditionalCommandScript : CommandBaseScript
    {
        public List<Button> buttons;
        public List<Button> skillButtons;  // size shoule bd 5 with order E R D F C

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
                }
                indexOfSkillButtons++;
            }
            for (int i = indexOfSkillButtons; i < skillButtons.Count; i++)
            {
                skillButtons[i].gameObject.SetActive(false);
            }
            // At this point, indexOfSkillButtons will be the amount of valid skill button.

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
                    ClearAllTargetDisplayUI();
                    foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList())
                    {
                        if (i.GetComponent<UnitBaseScript>() != null)
                        {
                            if (i.GetComponent<UnitBaseScript>().MoveAbility != null)
                            {
                                i.GetComponent<UnitBaseScript>().MoveAbility.LookAtTarget(followTarget.gameObject);
                            }
                            CreateGOToGOUI(i, followTarget.gameObject, Color.yellow);
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

        private void UseSkill(int numberOfSkills, int index)
        {
            if (index >= numberOfSkills)
            {
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
                return;
            }
            string skillType = showingAbilities.Keys.ToList()[index];
            float maxCoolDown = 0;
            foreach (SpecialAbilityBaseScript i in showingAbilities[skillType])
            {
                maxCoolDown = maxCoolDown > i.GetCoolDownPercent() ? maxCoolDown : i.GetCoolDownPercent();
            }
            Debug.Log($"Using skill{skillType} with CD = {maxCoolDown}");

            if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(NoSelectionSpecialAbilityScript)) != default)
            {
                UseNoSelectionSkill(showingAbilities[skillType].Where(x => x.GetType() == typeof(NoSelectionSpecialAbilityScript)).ToList());
            }
            else if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(SelectTargetSpecialAbilityScript)) != default)
            {
                UseSelectTargetSkill(showingAbilities[skillType].Where(x => x.GetType() == typeof(SelectTargetSpecialAbilityScript)).ToList());
            }
            else if (showingAbilities[skillType].FirstOrDefault(x => x.GetType() == typeof(SelectSpaceSpecialAbilityScript)) != default)
            {
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
            if (target != null && ((SelectTargetSpecialAbilityScript)abilities.FirstOrDefault()).possibleTargetTags.Contains(target.tag))
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
                        temp.UseAbility(target.gameObject);
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

        public void OnSkill1ButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill1;
        }

        public void OnSkill2ButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill2;
        }

        public void OnSkill3ButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill3;
        }

        public void OnSkill4ButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill4;
        }

        public void OnSkill5ButtonClicked()
        {
            InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Skill5;
        }
    }
}