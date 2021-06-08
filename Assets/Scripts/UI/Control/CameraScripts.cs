using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScripts : MonoBehaviour
{
    public float shiftSpeed;
    public float zoomSpeed;
    public float rotateSpeed;

    private bool isTracking = false;
    private Vector3 lastCenter = new Vector3(Mathf.Infinity, 0, 0);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Reset
        if (Input.GetKeyDown(InputManager.HotKeys.ResetCamera))
        {
            ResetCamera();
        }

        // Move
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width
            || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
        {
            return;
        }
        if (Input.mousePosition.x < 10 || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position -= transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }
        if (Input.mousePosition.x > Screen.width - 10 || Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }
        if (Input.mousePosition.y < 10 || Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }
        if (Input.mousePosition.y > Screen.height - 10 || Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            transform.position -= Quaternion.AngleAxis(-90, transform.right)
                * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            transform.position += Quaternion.AngleAxis(-90, transform.right) 
                * Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * transform.right * Time.deltaTime * shiftSpeed;
            isTracking = false;
        }

        // Zoom in and out
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            transform.position += transform.forward * Time.deltaTime * zoomSpeed;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            transform.position -= transform.forward * Time.deltaTime * zoomSpeed;
        }

        if (Input.GetKeyDown(InputManager.HotKeys.TrackSelectedUnits))
        {
            isTracking = true;
        }
        isTracking = isTracking && SelectControlScript.SelectionControlInstance.GetAllGameObjects().Count != 0;

        if (isTracking)
        {
            Vector3 center = SelectControlScript.SelectionControlInstance.FindCenter();
            // First tracking frame
            if (lastCenter.x == Mathf.Infinity)
            {
                transform.LookAt(center);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                lastCenter = center;
            }
            transform.position += center - lastCenter;
            lastCenter = center;
            if (Input.GetKey(InputManager.HotKeys.RotateCamera))
            {
                transform.RotateAround(center, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);
                transform.RotateAround(center, Vector3.Cross(Vector3.up, center - transform.position), -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }
        }
        else
        {
            // Enable self rotate
            if (Input.GetKey(InputManager.HotKeys.RotateCamera))
            {
                transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);
                transform.RotateAround(transform.position, Vector3.Cross(Vector3.up, transform.forward), -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }

            // Clear trakcing data
            lastCenter = new Vector3(Mathf.Infinity, 0, 0);
        }
    }

    private void ResetCamera()
    {
        transform.position = new Vector3(transform.position.x, 150, transform.position.z);
        transform.eulerAngles = new Vector3(45, 0, 0);

        isTracking = false;
        lastCenter = new Vector3(Mathf.Infinity, 0, 0);
    }
}
