using UnityEngine;
using System.Linq;
using RTS.UI.Command;
using System.Collections.Generic;

namespace RTS.UI.CameraView
{
    public class CameraControlScripts : MonoBehaviour
    {
        public static CameraControlScripts CameraControlScriptsInstance { get; private set; }
        public Transform cameraCenter;
        public float zoomSpeedStandardDistance;
        public float shiftSpeed;
        public float zoomSpeed;
        public float rotateSpeed;
        public bool TacticalView { get; private set; } = false;

        private bool isTracking = false;

        void Awake()
        {
            CameraControlScriptsInstance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(InputManager.HotKeys.TacticalView))
            {
                // zoom x0.1
                if (TacticalView)
                {

                }
                // zoom x10
                else
                {

                }
                TacticalView = !TacticalView;
            }

            transform.LookAt(cameraCenter);

            // Move
            if (!Input.GetKey(InputManager.HotKeys.RotateCamera) && !(Input.mousePosition.x < 0 || 
                Input.mousePosition.x > Screen.width || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height))
            {
                if (Input.mousePosition.x < 10 || Input.GetKey(KeyCode.LeftArrow))
                {
                    cameraCenter.position -= transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
                if (Input.mousePosition.x > Screen.width - 10 || Input.GetKey(KeyCode.RightArrow))
                {
                    cameraCenter.position += transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
                if (Input.mousePosition.y < 10 || Input.GetKey(KeyCode.DownArrow))
                {
                    cameraCenter.position -= Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
                if (Input.mousePosition.y > Screen.height - 10 || Input.GetKey(KeyCode.UpArrow))
                {
                    cameraCenter.position += Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
                if ((Input.GetAxis("Mouse ScrollWheel") > 0 && Input.GetKey(InputManager.HotKeys.SetCameraHeight)) ||
                    Input.GetKey(KeyCode.KeypadPlus))
                {
                    cameraCenter.position -= Quaternion.AngleAxis(-90, transform.right)
                        * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
                if ((Input.GetAxis("Mouse ScrollWheel") < 0 && Input.GetKey(InputManager.HotKeys.SetCameraHeight)) ||
                    Input.GetKey(KeyCode.KeypadMinus))
                {
                    cameraCenter.position += Quaternion.AngleAxis(-90, transform.right)
                        * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
                    isTracking = false;
                }
            }

            // Zoom in and out
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && !Input.GetKey(InputManager.HotKeys.SetCameraHeight))
            {
                transform.position += transform.forward * Time.deltaTime * zoomSpeed *
                    ((transform.position - cameraCenter.position).magnitude / zoomSpeedStandardDistance);
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && !Input.GetKey(InputManager.HotKeys.SetCameraHeight))
            {
                transform.position -= transform.forward * Time.deltaTime * zoomSpeed *
                    ((transform.position - cameraCenter.position).magnitude / zoomSpeedStandardDistance);
            }

            if (Input.GetKeyDown(InputManager.HotKeys.TrackSelectedUnits))
            {
                isTracking = true;
            }
            isTracking = isTracking && SelectControlScript.SelectionControlInstance.GetAllGameObjectsAsList().Count != 0 &&
                !SelectControlScript.SelectionControlInstance.SelectedChanged;

            if (isTracking)
            {
                cameraCenter.position = SelectControlScript.SelectionControlInstance.FindCenter();
            }

            // check camera position
            if (cameraCenter.position.magnitude > GameManager.GameManagerInstance.MapRadius)
            {
                cameraCenter.position = cameraCenter.position.normalized * GameManager.GameManagerInstance.MapRadius;
            }

            if (Input.GetKey(InputManager.HotKeys.RotateCamera))
            {
                transform.RotateAround(cameraCenter.position, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);

                Vector3 currentVector = transform.position - cameraCenter.position;
                Vector3 planeVector = currentVector;
                planeVector.y = 0;
                if (currentVector.y >= 0)
                {
                    float currentAngleX = Vector3.Angle(planeVector, currentVector);
                    float newAngleX = Mathf.Clamp(currentAngleX - Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -85, 85);
                    float trueRotate = newAngleX - currentAngleX;
                    transform.RotateAround(cameraCenter.position, Vector3.Cross(Vector3.up, cameraCenter.position - transform.position), trueRotate);
                }
                else
                {
                    float currentAngleX = -Vector3.Angle(planeVector, currentVector);
                    float newAngleX = Mathf.Clamp(currentAngleX - Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -85, 85);
                    float trueRotate = newAngleX - currentAngleX;
                    transform.RotateAround(cameraCenter.position, Vector3.Cross(Vector3.up, cameraCenter.position - transform.position), trueRotate);
                }
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            }
        }
    }
}