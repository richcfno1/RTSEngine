using RTS.Game.RTSGameObject;
using RTS.Game.UI.Command;
using RTS.Game.UI.TacticalView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTS.Game.UI
{
    public class InputManager : MonoBehaviour
    {
        public enum MousePosition
        {
            None,
            UI,
            SelfUnit,
            FriendUnit,
            NeutrualUnit,
            EnemyUnit
        }

        public enum MouseTexture
        {
            Normal,
            Command,
            ValidTarget
        }

        public enum CommandActionState
        {
            NoAction,
            Select,
            MainCommandMove,
            MainCommandAttack,
            MainCommandFollow,
            Stop,
            AttackWaitingNextKey,
            AttackTarget,
            AttackMoving,
            FollowWaitingNextKey,
            FollowTarget,
            Move,
            LookAtWaitingNextKey,
            LookAtTarget,
            LookAtSpace,
            Skill1,
            Skill2,
            Skill3,
            Skill4,
            Skill5
        }

        public static class HotKeys
        {
            // Unit
            // Select
            public static KeyCode SelectUnit = KeyCode.Mouse0;
            public static KeyCode SelectAllUnit = KeyCode.Q;
            public static KeyCode SelectSameType = KeyCode.W;  // Also support select main selected
            public static KeyCode MoveMainSelectTypeToNext = KeyCode.Tab;
            // Select specific unit or type in select panel
            public static KeyCode Select1 = KeyCode.F1;
            public static KeyCode Select2 = KeyCode.F2;
            public static KeyCode Select3 = KeyCode.F3;
            public static KeyCode Select4 = KeyCode.F4;
            public static KeyCode Select5 = KeyCode.F5;
            public static KeyCode Select6 = KeyCode.F6;
            public static KeyCode Select7 = KeyCode.F7;
            public static KeyCode Select8 = KeyCode.F8;
            public static KeyCode Select9 = KeyCode.F9;
            public static KeyCode Select10 = KeyCode.F10;
            // Team select
            public static KeyCode GroupKey = KeyCode.LeftControl;
            public static KeyCode GroupAddKey = KeyCode.LeftShift;
            public static KeyCode Group1 = KeyCode.Alpha1;
            public static KeyCode Group2 = KeyCode.Alpha2;
            public static KeyCode Group3 = KeyCode.Alpha3;
            public static KeyCode Group4 = KeyCode.Alpha4;
            public static KeyCode Group5 = KeyCode.Alpha5;
            public static KeyCode Group6 = KeyCode.Alpha6;
            public static KeyCode Group7 = KeyCode.Alpha7;
            public static KeyCode Group8 = KeyCode.Alpha8;
            public static KeyCode Group9 = KeyCode.Alpha9;
            public static KeyCode Group10 = KeyCode.Alpha0;

            // Action
            // Base mouse action (without button)
            public static KeyCode MainCommand = KeyCode.Mouse1;
            public static KeyCode CancelMainCommand = KeyCode.Mouse0;  // In fact, Key SelectUnit can achieve the cancel function
            public static KeyCode SelectTarget = KeyCode.Mouse0;
            public static KeyCode CancelSelectTarget = KeyCode.Mouse1;
            // Special key (without button) height setting
            public static KeyCode SetUnitMoveHeight = KeyCode.LeftShift;
            // Additional action supported by click 
            public static KeyCode Stop = KeyCode.S;
            public static KeyCode Attack = KeyCode.A;
            public static KeyCode Follow = KeyCode.Z;
            public static KeyCode LookAt = KeyCode.X;
            // Special Ability
            public static KeyCode Skill1 = KeyCode.E;
            public static KeyCode Skill2 = KeyCode.R;
            public static KeyCode Skill3 = KeyCode.D;
            public static KeyCode Skill4 = KeyCode.F;
            public static KeyCode Skill5 = KeyCode.C;
            // Unit Fire Control
            public static KeyCode FireControlKey = KeyCode.LeftControl;
            public static KeyCode Aggressive = KeyCode.A;
            public static KeyCode Neutral = KeyCode.S;
            public static KeyCode Passive = KeyCode.D;

            // Camera
            public static KeyCode RotateCamera = KeyCode.Mouse2;
            public static KeyCode SetCameraHeight = KeyCode.LeftShift;
            public static KeyCode TrackSelectedUnits = KeyCode.V;
            public static KeyCode TacticalView = KeyCode.Space;
        }
        public static InputManager InputManagerInstance { get; private set; }

        public Texture2D normalCursorTexture;
        public Texture2D commandCursorTexture;
        public Texture2D validTargetCursorTexture;
        public List<GameObject> notSelectUI;  // A list of UI component when mouse click on them, selection will not be called

        private GraphicRaycaster graphicRaycaster;
        private PointerEventData pointerEventData;
        private EventSystem eventSystem;

        public RTSGameObjectBaseScript PointedRTSGameObject { get; set; } = null;
        public MousePosition CurrentMousePosition { get; private set; } = MousePosition.None;
        public MouseTexture CurrentMouseTexture { get; set; } = MouseTexture.Normal;
        public MouseTexture LastMouseTexture { get; private set; } = MouseTexture.Normal;
        public CommandActionState CurrentCommandActionState { get; set; } = CommandActionState.NoAction;
        public CommandActionState LastCommandActionState { get; private set; } = CommandActionState.NoAction;

        void Awake()
        {
            InputManagerInstance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.Confined;

            graphicRaycaster = GetComponent<GraphicRaycaster>();
            eventSystem = GetComponent<EventSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            LastCommandActionState = CurrentCommandActionState;
            // DEBUG USE
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Determine mouse position
            CurrentMousePosition = MousePosition.None;
            PointedRTSGameObject = null;
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);
            foreach (RaycastResult result in results)
            {
                if (!result.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (notSelectUI.Contains(result.gameObject))
                {
                    CurrentMousePosition = MousePosition.UI;
                    break;
                }
                else if (result.gameObject.GetComponentInParent<TacticalViewSingleIconScript>() != null)
                {
                    PointedRTSGameObject = result.gameObject.GetComponentInParent<TacticalViewSingleIconScript>().bindObject;
                    if (result.gameObject.GetComponentInParent<TacticalViewSingleIconScript>().bindObject.BelongTo == 
                        GameManager.GameManagerInstance.SelfIndex)
                    {
                        CurrentMousePosition = MousePosition.SelfUnit;
                    }
                    else
                    {
                        CurrentMousePosition = MousePosition.EnemyUnit;
                    }
                }
            }

            if (CurrentMousePosition == MousePosition.None)
            {
                RTSGameObjectBaseScript temp = SingleSelectionHelper();
                if (temp != null)
                {
                    if (temp.BelongTo == GameManager.GameManagerInstance.SelfIndex)
                    {
                        CurrentMousePosition = MousePosition.SelfUnit;
                        PointedRTSGameObject = temp;
                    }
                    // TODO: Extend enemy to friend neutrual enemy
                    else
                    {
                        CurrentMousePosition = MousePosition.EnemyUnit;
                        PointedRTSGameObject = temp;
                    }
                }
            }

            if (CurrentMousePosition == MousePosition.UI || !SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
            {
                CurrentMouseTexture = MouseTexture.Normal;
            }

            switch (CurrentMouseTexture)
            {
                case MouseTexture.Normal:
                    if (LastMouseTexture != MouseTexture.Normal)
                    {
                        Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
                        LastMouseTexture = MouseTexture.Normal;
                    }
                    break;
                case MouseTexture.Command:
                    if (LastMouseTexture != MouseTexture.Command)
                    {
                        Cursor.SetCursor(commandCursorTexture, new Vector2(commandCursorTexture.width / 2, commandCursorTexture.height / 2),
                            CursorMode.Auto);
                        LastMouseTexture = MouseTexture.Command;
                    }
                    break;
                case MouseTexture.ValidTarget:
                    if (LastMouseTexture != MouseTexture.ValidTarget)
                    {
                        Cursor.SetCursor(validTargetCursorTexture, new Vector2(commandCursorTexture.width / 2, commandCursorTexture.height / 2),
                            CursorMode.Auto);
                        LastMouseTexture = MouseTexture.ValidTarget;
                    }
                    break;
            }
        }

        public static RTSGameObjectBaseScript SingleSelectionHelper()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray));
            hits.RemoveAll(x => x.collider.GetComponent<RTSGameObjectBaseScript>() == null);
            if (hits.Count == 0)
            {
                return null;
            }
            else if ((hits.Count == 1 || !hits[0].collider.CompareTag("Ship") || !hits[0].collider.CompareTag("Fighter")) && 
                hits[0].collider.GetComponent<MeshRenderer>().enabled && hits[0].collider.GetComponent<MeshRenderer>().isVisible)
            {
                return hits[0].collider.GetComponent<RTSGameObjectBaseScript>();
            }
            else if (hits.Count > 1 && hits[1].collider.CompareTag("Subsystem") && 
                hits[1].collider.GetComponent<MeshRenderer>().enabled && hits[1].collider.GetComponent<MeshRenderer>().isVisible)
            {
                return hits[1].collider.GetComponent<RTSGameObjectBaseScript>();
            }
            else if (hits[0].collider.GetComponent<MeshRenderer>().enabled && hits[0].collider.GetComponent<MeshRenderer>().isVisible)
            {
                return hits[0].collider.GetComponent<RTSGameObjectBaseScript>();
            }
            return null;
        }
    }
}
