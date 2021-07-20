using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.RTSGameObject;

namespace RTS.UI.CameraView
{
    public class CameraViewScript : MonoBehaviour
    {
        [Serializable]
        public class InfoData
        {
            public RTSGameObjectBaseScript.ObjectType type;
            public GameObject infoPrefab;
        }

        public float minInfoBarDisplaySize;
        public float maxInfoBarDisplaySize;
        public List<InfoData> infoPrefabs;
        public Transform Canvas;

        private Dictionary<int, GameObject> allInfoBar = new Dictionary<int, GameObject>();  // RTSGO index -> Info

        // Update is called once per frame
        void Update()
        {
            // Draw unit Info
            List<GameObject> visibleGameObjects = GameManager.GameManagerInstance.GetAllGameObjects().
                Where(x => x != null && x.GetComponent<Renderer>() != null && x.GetComponent<Renderer>().enabled &&
                x.GetComponent<Renderer>().isVisible).
                Where(x =>
                {
                    float scale = DistanceAndDiameterToPixelSize((x.transform.position - transform.position).magnitude,
                        x.GetComponent<RTSGameObjectBaseScript>().radius);
                    return scale >= minInfoBarDisplaySize && scale <= maxInfoBarDisplaySize;
                }).ToList();
            List<int> allVisibleIndex = new List<int>();
            foreach (GameObject i in visibleGameObjects)
            {
                RTSGameObjectBaseScript tempScript = i.GetComponent<RTSGameObjectBaseScript>();
                int tempIndex = tempScript.Index;
                RTSGameObjectBaseScript.ObjectType tempType = tempScript.objectType;
                InfoData tempInfoData = infoPrefabs.FirstOrDefault(x => x.type == tempType);
                GameObject tempPrefab;
                if (tempInfoData == default)
                {
                    continue;
                }
                else
                {
                    tempPrefab = tempInfoData.infoPrefab;
                }
                allVisibleIndex.Add(tempIndex);
                if (allInfoBar.ContainsKey(tempIndex))
                {
                    Vector3 position = Camera.main.WorldToScreenPoint(i.transform.position);
                    position.z = 0;
                    float scale = DistanceAndDiameterToPixelSize(
                        (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                    GameObject newBar = allInfoBar[tempIndex];
                    newBar.transform.position = position;
                    newBar.transform.localScale = Vector3.one * scale;
                    newBar.GetComponent<CameraViewInfoScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
                }
                else
                {
                    Vector3 position = Camera.main.WorldToScreenPoint(i.transform.position);
                    position.z = 0;
                    float scale = DistanceAndDiameterToPixelSize(
                        (i.transform.position - transform.position).magnitude, i.GetComponent<RTSGameObjectBaseScript>().radius);
                    GameObject newBar = Instantiate(tempPrefab, position, new Quaternion(), Canvas.transform);
                    newBar.transform.localScale = Vector3.one * scale;
                    newBar.GetComponent<CameraViewInfoScript>().hpdata.value = tempScript.HP / tempScript.maxHP;
                    Color tempColor = tempScript.BelongTo == GameManager.GameManagerInstance.selfIndex ? Color.green : Color.red;
                    tempColor.a = 0.5f;
                    newBar.GetComponent<CameraViewInfoScript>().ChangeColor(tempColor);
                    newBar.GetComponent<CameraViewInfoScript>().bindObject = i.GetComponent<RTSGameObjectBaseScript>();
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