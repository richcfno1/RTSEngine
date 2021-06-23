using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.RTSGameObject;

namespace RTS.UI.MainCamera
{
    public class CameraViewScript : MonoBehaviour
    {
        public float minInfoBarDisplaySize;
        public float maxInfoBarDisplaySize;
        public GameObject InfoBarPrefab;
        public Transform Canvas;

        private Dictionary<int, GameObject> allInfoBar = new Dictionary<int, GameObject>();  // RTSGO index -> Info bar

        // Update is called once per frame
        void Update()
        {
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
                    float scale = Mathf.Clamp(DistanceAndDiameterToPixelSize(
                        (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius),
                        0, maxInfoBarDisplaySize);
                    GameObject newBar = allInfoBar[tempIndex];
                    newBar.transform.position = Camera.main.WorldToScreenPoint(i.transform.position) + Vector3.up * scale * tempScript.ratio;
                    newBar.transform.localScale = InfoBarPrefab.transform.localScale * scale;
                    newBar.GetComponent<CameraViewInfoScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
                }
                else
                {
                    float scale = Mathf.Clamp(DistanceAndDiameterToPixelSize(
                        (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius),
                        0, maxInfoBarDisplaySize);
                    GameObject newBar = Instantiate(InfoBarPrefab,
                        Camera.main.WorldToScreenPoint(i.transform.position) + Vector3.up * scale * tempScript.ratio,
                        new Quaternion(), Canvas.transform);
                    newBar.transform.localScale = InfoBarPrefab.transform.localScale * DistanceAndDiameterToPixelSize(
                        (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                    newBar.GetComponent<CameraViewInfoScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
                    Color tempColor = tempScript.BelongTo == GameManager.GameManagerInstance.selfIndex ? Color.green : Color.red;
                    tempColor.a = 0.5f;
                    newBar.GetComponent<CameraViewInfoScript>().hpimage.color = tempColor;
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
            foreach (int i in toRemove)
            {
                allInfoBar.Remove(i);
            }
        }

        private float DistanceAndDiameterToPixelSize(float distance, float radius)
        {

            float pixelSize = (2 * radius * Mathf.Rad2Deg * Screen.height) / (distance * Camera.main.fieldOfView);
            return pixelSize;
        }
    }
}
