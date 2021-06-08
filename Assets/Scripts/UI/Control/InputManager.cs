using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager InputManagerInstance { get; private set; }
    public enum State
    {
        NoAction,  // NoAction is actually select nothing.
        Selecting
    }
    public static class HotKeys
    {
        public static KeyCode SelectUnit = KeyCode.Mouse0;
        public static KeyCode SetUnitMoveHeight = KeyCode.LeftShift;
        public static KeyCode MoveUnit = KeyCode.Mouse1;
        public static KeyCode StopUnit = KeyCode.S;

        public static KeyCode RotateCamera = KeyCode.Mouse2;
        public static KeyCode ResetCamera = KeyCode.B;
        public static KeyCode TrackSelectedUnits = KeyCode.V;
    }

    public List<GameObject> notSelectUI;  // A list of UI component when mouse click on them, selection will not be called
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private bool canSelect = true;

    public State CurrentState { get; set; }

    void Awake()
    {
        InputManagerInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        if (Input.GetKeyDown(HotKeys.SelectUnit))
        {
            canSelect = true;
            //Set up the new Pointer Event
            pointerEventData = new PointerEventData(eventSystem);
            //Set the Pointer Event Position to that of the mouse position
            pointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            raycaster.Raycast(pointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (notSelectUI.Contains(result.gameObject))
                {
                    canSelect = false;
                }
            }
        }
        switch (CurrentState)
        {
            case State.NoAction:
                // Select
                if (Input.GetKeyDown(HotKeys.SelectUnit) && canSelect)
                {
                    SelectControlScript.SelectionControlInstance.StartSelect();
                    CurrentState = State.Selecting;
                }
                break;
            case State.Selecting:
                if (Input.GetKeyUp(HotKeys.SelectUnit))
                {
                    SelectControlScript.SelectionControlInstance.EndSelect();
                    CurrentState = State.NoAction;
                }
                break;
        }
    }
}
