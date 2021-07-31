using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.Ability.SpecialAbility;
using RTS.RTSGameObject;
using RTS.RTSGameObject.Unit;

namespace RTS.UI.Command
{
    public class SelectControlScript : MonoBehaviour
    {
        public static SelectControlScript SelectionControlInstance { get; private set; }

        public GameObject SelectionBoxPrefab;

        // TODO: add a comparator here
        public SortedDictionary<string, List<GameObject>> SelectedGameObjects { get; private set; } = new SortedDictionary<string, List<GameObject>>();
        public string MainSelectedType { get; private set; } = default;
        public GameObject MainSelectedGameObject { get; private set; } = null;
        public bool SelectedChanged { get; private set; } = false;
        public bool SelectedOwnUnits { get; private set; } = false;
        public List<List<GameObject>> UnitGroup { get; private set; } = Enumerable.Repeat(new List<GameObject>(), 10).ToList();

        private int selfIndex;
        private Vector3 mouseStartPosition;
        private Vector3 mouseEndPosition;
        private bool mouseLeftUp;
        private bool mouseLeftDown;
        private List<GameObject> allSelectableGameObjects;

        private GameObject SelectionBox = null;

        void Awake()
        {
            SelectionControlInstance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            selfIndex = GameManager.GameManagerInstance.selfIndex;
            mouseStartPosition = Input.mousePosition;
        }

        // Update is called once per frame
        void Update()
        {
            allSelectableGameObjects = GameManager.GameManagerInstance.GetAllGameObjects();
            if (Input.GetKeyDown(InputManager.HotKeys.SelectUnit) && CanSelect())
            {
                mouseStartPosition = Input.mousePosition;
                mouseLeftUp = false;
                mouseLeftDown = true;
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.Select;
            }
            else if (Input.GetKeyUp(InputManager.HotKeys.SelectUnit) && InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.Select)
            {
                mouseEndPosition = Input.mousePosition;
                mouseLeftUp = true;
                mouseLeftDown = false;
                InputManager.InputManagerInstance.CurrentCommandActionState = InputManager.CommandActionState.NoAction;
            }
            if (mouseLeftDown)
            {
                // Draw box
                Vector3 boxPosition = (Input.mousePosition + mouseStartPosition) / 2;
                float boxSizeX = Mathf.Abs(Input.mousePosition.x - mouseStartPosition.x);
                float boxSizeY = Mathf.Abs(Input.mousePosition.y - mouseStartPosition.y);
                if (SelectionBox == null)
                {
                    SelectionBox = Instantiate(SelectionBoxPrefab, transform);
                }
                SelectionBox.transform.position = boxPosition;
                SelectionBox.transform.localScale = new Vector3(boxSizeX, boxSizeY);
            }
            if (mouseLeftUp)
            {
                mouseLeftUp = false;
                Judge();
                Destroy(SelectionBox);
                SelectedChanged = true;
            }
            else
            {
                SelectedChanged = false;
            }

            if (Input.GetKeyDown(InputManager.HotKeys.SelectAllUnit))
            {
                SetSelectedGameObjects(GameManager.GameManagerInstance.GetGameObjectForPlayer(
                    GameManager.GameManagerInstance.selfIndex).Where(x => x != null && x.GetComponent<UnitBaseScript>() != null).ToList());
            }

            if (Input.GetKeyDown(InputManager.HotKeys.SelectSameType) && SelectedOwnUnits)
            {
                List<GameObject> tempSameType = GameManager.GameManagerInstance.GetGameObjectForPlayer(GameManager.GameManagerInstance.selfIndex);
                string targetType = MainSelectedType;
                if (MainSelectedGameObject != null)
                {
                    targetType = MainSelectedGameObject.GetComponent<UnitBaseScript>().UnitTypeID;
                }
                tempSameType = tempSameType.Where(x => x != null && x.GetComponent<UnitBaseScript>() != null && 
                    x.GetComponent<UnitBaseScript>().UnitTypeID == targetType).ToList();
                SetSelectedGameObjects(tempSameType);
            }

            // Remove destroyed object and add highlighted outline to others
            List<string> toRemoveKey = new List<string>();
            foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
            {
                List<GameObject> toRemoveValue = new List<GameObject>();
                // Check objects with same type
                foreach (GameObject j in i.Value)
                {
                    if (j == null)
                    {
                        toRemoveValue.Add(j);
                    }
                }
                foreach (GameObject j in toRemoveValue)
                {
                    i.Value.Remove(j);
                }
                if (i.Value.Count == 0)
                {
                    toRemoveKey.Add(i.Key);
                }
            }
            foreach (string i in toRemoveKey)
            {
                SelectedGameObjects.Remove(i);
            }

            foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
            {
                i.Value.Sort((x, y) => x.GetComponent<RTSGameObjectBaseScript>().Index.
                    CompareTo(y.GetComponent<RTSGameObjectBaseScript>().Index));
            }

                if (MainSelectedType == null || !SelectedGameObjects.Keys.Contains(MainSelectedType))
            {
                MainSelectedType = SelectedGameObjects.Keys.FirstOrDefault();
            }

            int selectSpecificIndex = -1;
            if (Input.GetKeyDown(InputManager.HotKeys.Select1))
            {
                selectSpecificIndex = 0;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select2))
            {
                selectSpecificIndex = 1;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select3))
            {
                selectSpecificIndex = 2;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select4))
            {
                selectSpecificIndex = 3;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select5))
            {
                selectSpecificIndex = 4;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select6))
            {
                selectSpecificIndex = 5;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select7))
            {
                selectSpecificIndex = 6;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select8))
            {
                selectSpecificIndex = 7;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select9))
            {
                selectSpecificIndex = 8;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Select10))
            {
                selectSpecificIndex = 9;
            }

            if (SelectedGameObjects.Keys.Count > 1)
            {
                if (Input.GetKeyDown(InputManager.HotKeys.MoveMainSelectTypeToNext))
                {
                    List<string> tempSortedTypes = SelectedGameObjects.Keys.ToList();
                    int index = tempSortedTypes.IndexOf(MainSelectedType) + 1;
                    index = index >= tempSortedTypes.Count ? 0 : index;
                    MainSelectedType = tempSortedTypes[index];
                }
                if (selectSpecificIndex != -1 && selectSpecificIndex < SelectedGameObjects.Keys.Count)
                {
                    List<string> tempSortedTypes = SelectedGameObjects.Keys.ToList();
                    if (MainSelectedType == tempSortedTypes[selectSpecificIndex])
                    {
                        SetSelectedGameObjects(SelectedGameObjects[MainSelectedType]);
                    }
                    else
                    {
                        MainSelectedType = tempSortedTypes[selectSpecificIndex];
                    }
                }


            }
            else
            {
                List<GameObject> tempAllGameObjects = GetAllGameObjectsAsList();
                if (Input.GetKeyDown(InputManager.HotKeys.MoveMainSelectTypeToNext))
                {
                    if (MainSelectedGameObject == null)
                    {
                        MainSelectedGameObject = tempAllGameObjects.FirstOrDefault();
                    }
                    else
                    {
                        int index = tempAllGameObjects.IndexOf(MainSelectedGameObject) + 1;
                        MainSelectedGameObject = index >= tempAllGameObjects.Count ? null : tempAllGameObjects[index];
                    }
                }
                if (selectSpecificIndex != -1 && selectSpecificIndex < tempAllGameObjects.Count)
                {
                    GameObject temp = tempAllGameObjects[selectSpecificIndex];
                    if (MainSelectedGameObject == temp)
                    {
                        SetSelectedGameObjects(new List<GameObject>() { MainSelectedGameObject });
                    }
                    else
                    {
                        MainSelectedGameObject = temp;
                    }
                }
            }

            int groupIndex = -1;
            if (Input.GetKeyDown(InputManager.HotKeys.Group1))
            {
                groupIndex = 0;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group2))
            {
                groupIndex = 1;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group3))
            {
                groupIndex = 2;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group4))
            {
                groupIndex = 3;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group5))
            {
                groupIndex = 4;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group6))
            {
                groupIndex = 5;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group7))
            {
                groupIndex = 6;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group8))
            {
                groupIndex = 7;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group9))
            {
                groupIndex = 8;
            }
            if (Input.GetKeyDown(InputManager.HotKeys.Group10))
            {
                groupIndex = 9;
            }
            if (groupIndex != -1)
            {
                if (SelectedOwnUnits)
                {
                    if (Input.GetKey(InputManager.HotKeys.GroupKey))
                    {
                        UnitGroup[groupIndex].Clear();
                        UnitGroup[groupIndex] = GetAllGameObjectsAsList();
                        UnitGroup[groupIndex].Sort((x, y) => x.GetComponent<RTSGameObjectBaseScript>().Index.
                            CompareTo(y.GetComponent<RTSGameObjectBaseScript>().Index));
                    }
                    else if (Input.GetKey(InputManager.HotKeys.GroupAddKey))
                    {
                        UnitGroup[groupIndex].AddRange(GetAllGameObjectsAsList());
                        UnitGroup[groupIndex].Sort((x, y) => x.GetComponent<RTSGameObjectBaseScript>().Index.
                            CompareTo(y.GetComponent<RTSGameObjectBaseScript>().Index));
                    }
                }
                SetSelectedGameObjects(UnitGroup[groupIndex]);
            }
        }

        private void ClearSelectedGameObjects()
        {
            foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
            {
                // Check objects with same type
                foreach (GameObject j in i.Value)
                {
                    if (j.GetComponent<cakeslice.Outline>() != null)
                    {
                        Destroy(j.GetComponent<cakeslice.Outline>());
                    }
                }
            }
            SelectedGameObjects.Clear();
            MainSelectedType = default;
            MainSelectedGameObject = null;
            SelectedOwnUnits = false;
        }

        private bool CanSelect()
        {
            return InputManager.InputManagerInstance.CurrentMousePosition != InputManager.MousePosition.UI &&
                // Main command
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.NoAction &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.NoAction) ||
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.MainCommandMove &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.MainCommandMove) ||
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.MainCommandFollow &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.MainCommandFollow) ||
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.MainCommandAttack &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.MainCommandAttack) ||
                // Command which does not need left click
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.Move &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.Move) ||
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.LookAtSpace &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.LookAtSpace) ||
                (InputManager.InputManagerInstance.CurrentCommandActionState == InputManager.CommandActionState.AttackMoving &&
                InputManager.InputManagerInstance.LastCommandActionState == InputManager.CommandActionState.AttackMoving);
        }

        private void Judge()
        {
            ClearSelectedGameObjects();
            if (mouseStartPosition != mouseEndPosition)
            {
                for (int i = 0; i < allSelectableGameObjects.Count; i++)
                {
                    if (allSelectableGameObjects[i] == null)
                    {
                        continue;
                    }
                    Vector2 position2D = Camera.main.WorldToScreenPoint(allSelectableGameObjects[i].transform.position);
                    if ((position2D.x >= mouseStartPosition.x && position2D.x <= mouseEndPosition.x && position2D.y >= mouseStartPosition.y && position2D.y <= mouseEndPosition.y) ||
                        (position2D.x >= mouseStartPosition.x && position2D.x <= mouseEndPosition.x && position2D.y <= mouseStartPosition.y && position2D.y >= mouseEndPosition.y) ||
                        (position2D.x <= mouseStartPosition.x && position2D.x >= mouseEndPosition.x && position2D.y >= mouseStartPosition.y && position2D.y <= mouseEndPosition.y) ||
                        (position2D.x <= mouseStartPosition.x && position2D.x >= mouseEndPosition.x && position2D.y <= mouseStartPosition.y && position2D.y >= mouseEndPosition.y))
                    {
                        if (allSelectableGameObjects[i].GetComponent<UnitBaseScript>() != null &&
                            selfIndex == allSelectableGameObjects[i].GetComponent<UnitBaseScript>().BelongTo)
                        {
                            AddGameObject(allSelectableGameObjects[i]);
                            SelectedOwnUnits = true;
                        }
                    }
                }
            }
            // Single selection
            else
            {
                RTSGameObjectBaseScript selected = InputManager.InputManagerInstance.PointedRTSGameObject;
                if (selected != null)
                {
                    AddGameObject(selected.gameObject);
                    SelectedOwnUnits = selected.BelongTo == selfIndex && selected.GetComponent<UnitBaseScript>() != null;
                }
            }
        }

        public List<GameObject> GetAllGameObjectsAsList()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
            {
                result.AddRange(i.Value);
            }
            return result;
        }

        public SortedDictionary<string, List<SpecialAbilityBaseScript>> GetAllSpecialAbilityOfMainSelected()
        {
            if (MainSelectedGameObject != null)
            {
                if (MainSelectedGameObject.GetComponent<UnitBaseScript>() != null)
                {
                    return MainSelectedGameObject.GetComponent<UnitBaseScript>().SpecialAbilityList;
                }
            }
            else if (MainSelectedType != default)
            {
                SortedDictionary<string, List<SpecialAbilityBaseScript>> result = new SortedDictionary<string, List<SpecialAbilityBaseScript>>();
                foreach (GameObject i in SelectedGameObjects[MainSelectedType])
                {
                    if (i.GetComponent<UnitBaseScript>() == null)
                    {
                        return new SortedDictionary<string, List<SpecialAbilityBaseScript>>();
                    }
                    foreach (KeyValuePair<string, List<SpecialAbilityBaseScript>> j in i.GetComponent<UnitBaseScript>().SpecialAbilityList)
                    {
                        if (result.ContainsKey(j.Key))
                        {
                            result[j.Key].AddRange(j.Value);
                        }
                        else
                        {
                            result.Add(j.Key, new List<SpecialAbilityBaseScript>(j.Value));
                        }
                    }
                }
                return result;
            }
            return new SortedDictionary<string, List<SpecialAbilityBaseScript>>();
        }

        public void AddGameObject(GameObject obj)
        {
            if (obj == null || obj.GetComponent<RTSGameObjectBaseScript>() == null)
            {
                return;
            }
            if (obj.GetComponent<UnitBaseScript>() != null)
            {
                string type = obj.GetComponent<UnitBaseScript>().UnitTypeID;
                if (SelectedGameObjects.ContainsKey(type))
                {
                    SelectedGameObjects[type].Add(obj);
                }
                else
                {
                    SelectedGameObjects[type] = new List<GameObject>() { obj };
                }
                obj.AddComponent<cakeslice.Outline>();
            }
            else
            {
                string type = obj.GetComponent<RTSGameObjectBaseScript>().typeID;
                if (SelectedGameObjects.ContainsKey(type))
                {
                    SelectedGameObjects[type].Add(obj);
                }
                else
                {
                    SelectedGameObjects[type] = new List<GameObject>() { obj };
                }
                obj.AddComponent<cakeslice.Outline>();
            }
        }

        public Vector3 FindCenter()
        {
            Vector3 result = new Vector3();
            int count = 0;
            foreach (KeyValuePair<string, List<GameObject>> i in SelectedGameObjects)
            {
                // Check objects with same type
                foreach (GameObject j in i.Value)
                {
                    result += j.transform.position;
                    count++;
                }
            }
            if (count == 0)
            {
                result.x = Mathf.Infinity;
                return result;
            }
            else
            {
                return result / count;
            }
        }

        public void SetSelectedGameObjects(List<GameObject> gameObjects)
        {
            ClearSelectedGameObjects();
            if (gameObjects.Count > 1)
            {
                foreach (GameObject i in gameObjects)
                {
                    if (i != null && i.GetComponent<UnitBaseScript>() != null && selfIndex == i.GetComponent<UnitBaseScript>().BelongTo)
                    {
                        AddGameObject(i);
                        SelectedOwnUnits = true;
                    }
                }
            }
            else if (gameObjects.Count == 1)
            {
                GameObject selected = gameObjects[0];
                if (selected != null)
                {
                    AddGameObject(selected);
                    SelectedOwnUnits = selected.GetComponent<RTSGameObjectBaseScript>().BelongTo == selfIndex
                        && selected.GetComponent<UnitBaseScript>() != null;
                }
            }
        }

        public void SetMainSelectedType(string target)
        {
            if (SelectedGameObjects.ContainsKey(target))
            {
                MainSelectedType = target;
            }
        }

        public void SetMainSelectedGameObject(GameObject target)
        {
            if (GetAllGameObjectsAsList().Contains(target))
            {
                MainSelectedGameObject = target;
            }
        }
    }
}
