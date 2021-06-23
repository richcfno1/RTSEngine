using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability;
using RTS.RTSGameObject;

namespace RTS.UI.Control
{
    public class AttackControlScript : MonoBehaviour
    {
        private class AttackUI
        {
            public GameObject from;
            public GameObject to;
            public GameObject circle;
            public GameObject line;

            public AttackUI(GameObject from, GameObject to, GameObject circlePrefab, GameObject linePrefab)
            {
                this.from = from;
                this.to = to;
                circle = Instantiate(circlePrefab, Vector3.zero, Quaternion.AngleAxis(90, Vector3.left), GameObject.Find("UI").transform);
                circle.transform.localScale *= to.GetComponent<Collider>().bounds.size.magnitude * 2;
                line = Instantiate(linePrefab, Vector3.zero, new Quaternion(), GameObject.Find("UI").transform);
            }

            // Return false if any of from or to is destoryed
            public bool Update()
            {
                if (from != null && to != null)
                {
                    circle.transform.position = to.transform.position;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, from.transform.position);
                    lineRenderer.SetPosition(1, to.transform.position);
                    return true;
                }
                else
                {
                    Destroy();
                    return false;
                }
            }

            public void Destroy()
            {
                GameObject.Destroy(circle);
                GameObject.Destroy(line);
            }
        }

        public float displayTime;
        public GameObject attackUICirclePrefab;
        public GameObject attackUILinePrefab;

        private List<AttackUI> uiGameObjects = new List<AttackUI>();
        private int selfIndex;

        void Start()
        {
            selfIndex = GameManager.GameManagerInstance.selfIndex;
        }

        // Update is called once per frame
        void Update()
        {
            if (InputManager.InputManagerInstance.CurrentState == InputManager.State.NoAction)
            {
                if (SelectControlScript.SelectionControlInstance.SelectedOwnUnits)
                {
                    if (Input.GetKeyDown(InputManager.HotKeys.AttackUnit) && InputManager.InputManagerInstance.EnableAction)
                    {
                        GameObject temp = SingleSelectionHelper();
                        if (temp != null && temp.GetComponent<RTSGameObjectBaseScript>().BelongTo != selfIndex)
                        {
                            ClearAttackUI();
                            foreach (GameObject i in SelectControlScript.SelectionControlInstance.GetAllGameObjects())
                            {
                                if (i.GetComponent<AttackAbilityScript>() != null)
                                {
                                    i.GetComponent<AttackAbilityScript>().UseAbility(new List<object>() { AttackAbilityScript.UseType.Specific, temp });
                                    CreateAttackUI(i, temp);
                                }
                            }
                            StartCoroutine(ClearUI(displayTime));
                        }
                    }
                }
            }
            if (uiGameObjects.Count != 0)
            {
                UpdateAttackUI();
            }
        }

        private IEnumerator ClearUI(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ClearAttackUI();
        }

        private void CreateAttackUI(GameObject from, GameObject to)
        {
            StopAllCoroutines();
            uiGameObjects.Add(new AttackUI(from, to, attackUICirclePrefab, attackUILinePrefab));
        }

        private void UpdateAttackUI()
        {
            List<AttackUI> toRemove = new List<AttackUI>();
            foreach (AttackUI i in uiGameObjects)
            {
                if (!i.Update())
                {
                    toRemove.Add(i);
                }
            }
            uiGameObjects.RemoveAll(x => toRemove.Contains(x));
        }
        private void ClearAttackUI()
        {
            foreach (AttackUI i in uiGameObjects)
            {
                i.Destroy();
            }
            uiGameObjects.Clear();
        }

        private GameObject SingleSelectionHelper()
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
