using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class CameraScripts : MonoBehaviour
{
    public float shiftSpeed;
    public float zoomSpeed;
    public float rotateSpeed;

    public float minInfoBarDisplaySize;
    public GameObject InfoBarPrefab;
    public Transform Canvas;

    private bool isTracking = false;
    private Vector3 lastCenter = new Vector3(Mathf.Infinity, 0, 0);

    private Dictionary<int, GameObject> allInfoBar = new Dictionary<int, GameObject>();  // RTSGO index -> Info bar

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
        if (!Input.GetKey(InputManager.HotKeys.RotateCamera))
        {
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

                Vector3 currentVector = transform.position - center;
                Vector3 planeVector = currentVector;
                planeVector.y = 0;
                if (currentVector.y >= 0)
                {
                    float currentAngleX = Vector3.Angle(planeVector, currentVector);
                    float newAngleX = Mathf.Clamp(currentAngleX - Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -85, 85);
                    float trueRotate = newAngleX - currentAngleX;
                    transform.RotateAround(center, Vector3.Cross(Vector3.up, center - transform.position), trueRotate);
                }
                else
                {
                    float currentAngleX = -Vector3.Angle(planeVector, currentVector);
                    float newAngleX = Mathf.Clamp(currentAngleX - Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -85, 85);
                    float trueRotate = newAngleX - currentAngleX;
                    transform.RotateAround(center, Vector3.Cross(Vector3.up, center - transform.position), trueRotate);
                }
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            }
        }
        else
        {
            // Enable self rotate
            float rotationX = transform.localEulerAngles.x;
            if (rotationX > 180)
            {
                rotationX -= 360;
            }
            if (Input.GetKey(InputManager.HotKeys.RotateCamera))
            {
                transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);
                rotationX += -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
                rotationX = Mathf.Clamp(rotationX, -90, 90);
                transform.localEulerAngles = new Vector3(rotationX, transform.localEulerAngles.y, 0);
            }

            // Clear trakcing data
            lastCenter = new Vector3(Mathf.Infinity, 0, 0);
        }

        // Draw unit Info
        List<GameObject> visibleGameObjects = GameManager.GameManagerInstance.GetAllGameObjects().
            Where(x => x != null && x.GetComponent<Renderer>() != null && x.GetComponent<Renderer>().enabled && 
            x.GetComponent<Renderer>().isVisible).
            Where(x => DistanceAndDiameterToPixelSize((x.transform.position - transform.position).magnitude, 
            x.GetComponent<RTSGameObjectBaseScript>().radius) >= minInfoBarDisplaySize).ToList();
        List<int> allVisibleIndex = new List<int>();
        foreach (GameObject i in visibleGameObjects)
        {
            RTSGameObjectBaseScript tempScript = i.GetComponent<RTSGameObjectBaseScript>();
            int tempIndex = tempScript.Index;
            allVisibleIndex.Add(tempIndex);
            if (allInfoBar.ContainsKey(tempIndex))
            {
                float scale = DistanceAndDiameterToPixelSize(
                    (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                GameObject newBar = allInfoBar[tempIndex];
                newBar.transform.position = Camera.main.WorldToScreenPoint(i.transform.position) + Vector3.up * scale * tempScript.ratio;
                newBar.transform.localScale = InfoBarPrefab.transform.localScale * scale;
                newBar.GetComponent<InfoBarScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
            }
            else
            {
                float scale = DistanceAndDiameterToPixelSize(
                    (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                GameObject newBar = Instantiate(InfoBarPrefab, 
                    Camera.main.WorldToScreenPoint(i.transform.position) + Vector3.up * scale * tempScript.ratio, 
                    new Quaternion(), Canvas.transform);
                newBar.transform.localScale = InfoBarPrefab.transform.localScale * DistanceAndDiameterToPixelSize(
                    (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                newBar.GetComponent<InfoBarScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
                Color tempColor = tempScript.BelongTo == GameManager.GameManagerInstance.selfIndex ? Color.green : Color.red;
                tempColor.a = 0.5f;
                newBar.GetComponent<InfoBarScript>().hpimage.color = tempColor;
                allInfoBar.Add(tempIndex, newBar);
            }
        }
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, GameObject> i in allInfoBar)
        {
            if (!allVisibleIndex.Contains(i.Key))
            {
                if (i.Value != null)
                {
                    Destroy(i.Value);
                }
                toRemove.Add(i.Key);
            }
        }
        foreach(int i in toRemove)
        {
            allInfoBar.Remove(i);
        }
    }

    private float DistanceAndDiameterToPixelSize(float distance, float radius)
    {

        float pixelSize = (2 * radius * Mathf.Rad2Deg * Screen.height) / (distance * Camera.main.fieldOfView);
        return pixelSize;
    }

    private void ResetCamera()
    {
        transform.position = new Vector3(transform.position.x, 150, transform.position.z);
        transform.eulerAngles = new Vector3(45, -45, 0);

        isTracking = false;
        lastCenter = new Vector3(Mathf.Infinity, 0, 0);
    }
}
