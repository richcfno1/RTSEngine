using RTS.RTSGameObject;
using RTS.UI.CameraView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTS.UI.Command
{
    public class InputManager : MonoBehaviour
    {
        public enum MousePosition
        {
            None,
            UI,
            SelfUnit,
            FriendUnit,
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

            // Camera
            public static KeyCode RotateCamera = KeyCode.Mouse2;
            public static KeyCode SetCameraHeight = KeyCode.LeftShift;
            public static KeyCode TrackSelectedUnits = KeyCode.V;
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
                else if (result.gameObject.GetComponentInParent<CameraViewInfoScript>() != null)
                {
                    PointedRTSGameObject = result.gameObject.GetComponentInParent<CameraViewInfoScript>().bindObject;
                    if (result.gameObject.GetComponentInParent<CameraViewInfoScript>().bindObject.BelongTo == 
                        GameManager.GameManagerInstance.selfIndex)
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
                    if (temp.BelongTo == GameManager.GameManagerInstance.selfIndex)
                    {
                        CurrentMousePosition = MousePosition.SelfUnit;
                        PointedRTSGameObject = temp;
                    }
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
            else if (hits.Count == 1 || !hits[0].collider.CompareTag("Ship"))
            {
                return hits[0].collider.GetComponent<RTSGameObjectBaseScript>();
            }
            else
            {
                if (hits[1].collider.CompareTag("Subsystem"))
                {
                    return hits[1].collider.GetComponent<RTSGameObjectBaseScript>();
                }
                return hits[0].collider.GetComponent<RTSGameObjectBaseScript>();
            }
        }
    }
}
