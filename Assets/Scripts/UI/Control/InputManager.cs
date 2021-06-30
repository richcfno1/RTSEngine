using RTS.RTSGameObject;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTS.UI.Control
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager InputManagerInstance { get; private set; }
        public enum State
        {
            NoAction,
            Selecting,
            Moving
        }
        public static class HotKeys
        {
            public static KeyCode SelectUnit = KeyCode.Mouse0;
            public static KeyCode SetUnitMoveHeight = KeyCode.LeftShift;
            public static KeyCode MoveUnit = KeyCode.Mouse1;
            public static KeyCode StopUnit = KeyCode.S;
            public static KeyCode AttackUnit = KeyCode.Mouse1;

            public static KeyCode RotateCamera = KeyCode.Mouse2;
            public static KeyCode SetCameraHeight = KeyCode.LeftShift;
            public static KeyCode TrackSelectedUnits = KeyCode.D;
        }

        public Texture2D cursorTexture;

        public List<GameObject> notSelectUI;  // A list of UI component when mouse click on them, selection will not be called
        private GraphicRaycaster raycaster;
        private PointerEventData pointerEventData;
        private EventSystem eventSystem;

        public State CurrentState { get; set; }
        public bool EnableAction { get; private set; }

        void Awake()
        {
            InputManagerInstance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
            CurrentState = State.NoAction;

            raycaster = GetComponent<GraphicRaycaster>();
            eventSystem = GetComponent<EventSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            EnableAction = true;
            //Set up the new Pointer Event
            pointerEventData = new PointerEventData(eventSystem);
            //Set the Pointer Event Position to that of the mouse position
            pointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            raycaster.Raycast(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                if (notSelectUI.Contains(result.gameObject))
                {
                    EnableAction = false;
                }
            }
        }

        public static GameObject SingleSelectionHelper()
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
                return hits[0].collider.gameObject;
            }
            else
            {
                if (hits[1].collider.CompareTag("Subsystem"))
                {
                    return hits[1].collider.gameObject;
                }
                return hits[0].collider.gameObject;
            }
        }
    }
}
