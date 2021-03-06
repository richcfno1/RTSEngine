using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.Game.RTSGameObject.Unit;

namespace RTS.Game.RTSGameObject.Subsystem
{
    public class CarrierSubsystemBaseScript : SubsystemBaseScript
    {
        public List<string> products;
        public int carrierCapacity;
        public float deployTime;
        public List<Vector3> deployPath;
        public float recallTime;
        public List<Vector3> recallPath;
        public float deployAndRecallSpeed;

        [HideInInspector]
        public Dictionary<string, List<GameObject>> deployedUnits = new Dictionary<string, List<GameObject>>();
        public Dictionary<string, int> carriedUnits = new Dictionary<string, int>();

        // These are values between 0 and 1 indicate the process of actions
        public float DeployProgress { get; private set; } = 0;
        public float RecallProgress { get; private set; } = 0;
        public float ProduceProgress { get; private set; } = 0;

        private Queue<string> produceQueue = new Queue<string>();
        private Queue<string> deployQueue = new Queue<string>();

        private bool isDeploying = false;
        private bool isRecalling = false;
        private bool isProducing = false;

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
            if (HP <= 0)
            {
                OnDestroyedAction();
            }
            if (!Active && HP / maxHP > repairPercentRequired)
            {
                OnSubsystemRepairedAction();
            }
            foreach (KeyValuePair<string, List<GameObject>> i in deployedUnits)
            {
                i.Value.RemoveAll(x => x == null);
            }

            if (!isProducing && produceQueue.Count != 0)
            {
                string unitType = produceQueue.Peek();
                string baseTypeName = GameManager.GameManagerInstance.UnitLibrary[unitType].baseTypeName;
                isProducing = true;
                StartCoroutine(FinishProduceAfter(unitType, Resources.Load<GameObject>(
                    GameManager.GameManagerInstance.GameObjectLibrary[baseTypeName]).GetComponent<UnitBaseScript>().buildTime));
            }
            if (!isDeploying && deployQueue.Count != 0)
            {
                string unitType = deployQueue.Peek();
                isDeploying = true;
                StartCoroutine(FinishDeployAfter(unitType, deployTime));
            }
        }

        private IEnumerator FinishDeployAfter(string type, float waitTime)
        {
            GameObject temp = GameManager.GameManagerInstance.InstantiateUnit(type,
                transform.TransformPoint(transform.localPosition),
                transform.rotation, GameObject.Find("RTSGameObject").transform, BelongTo,
                new Dictionary<string, string>());
            temp.GetComponent<UnitBaseScript>().Stop();
            Vector3 offset = Vector3.zero;
            foreach (Vector3 i in deployPath)
            {
                offset += i;
                temp.GetComponent<UnitBaseScript>().ForcedMove(transform.TransformPoint(i), deployAndRecallSpeed, false);
            }
            if (temp.GetComponent<UnitBaseScript>().MoveAbility != null)
            {
                temp.GetComponent<UnitBaseScript>().MoveAbility.Follow(Host.Index, offset, false);
            }
            carriedUnits[type]--;
            deployedUnits[type].Add(temp);
            deployQueue.Dequeue();
            for (float timer = waitTime; timer > 0; timer -= Time.fixedDeltaTime)
            {
                DeployProgress = 1 - timer / waitTime;
                yield return null;
            }
            DeployProgress = 0;
            isDeploying = false;
        }

        private IEnumerator FinishRecallAfter(string type, float waitTime)
        {
            // TODO Recall
            return null;
        }

        private IEnumerator FinishProduceAfter(string type, float waitTime)
        {
            for (float timer = waitTime; timer > 0; timer -= Time.fixedDeltaTime)
            {
                ProduceProgress = 1 - timer / waitTime;
                yield return null;
            }
            ProduceProgress = 0;
            produceQueue.Dequeue();
            carriedUnits[type]++;
            isProducing = false;
        }

        // Do I really need set target for this subsystem..?
        public override void SetTarget(List<object> target)
        {
            base.SetTarget(target);
        }

        protected override void OnCreatedAction()
        {
            base.OnCreatedAction();
            foreach (string i in products)
            {
                carriedUnits.Add(i, 0);
                deployedUnits.Add(i, new List<GameObject>());
            }
        }

        protected override void OnDestroyedAction()
        {
            base.OnDestroyedAction();
            StopAllCoroutines();
        }

        public virtual bool Produce(string type)
        {
            int count = 0;
            foreach (KeyValuePair<string, int> i in carriedUnits)
            {
                count += i.Value;
            }
            foreach (KeyValuePair<string, List<GameObject>> i in deployedUnits)
            {
                count += i.Value.Count;
            }
            if (count + produceQueue.Count < carrierCapacity)
            {
                if (products.Contains(type) && GameManager.GameManagerInstance.UnitLibrary.ContainsKey(type))
                {
                    produceQueue.Enqueue(type);
                    return true;
                }
            }
            return false;
        }

        public virtual bool Deploy(string type)
        {
            if (carriedUnits.ContainsKey(type))
            {
                if (carriedUnits[type] - deployQueue.Where(x => x == type).Count() > 0)
                {
                    deployQueue.Enqueue(type);
                    return true;
                }
            }
            return false;
        }

        public virtual bool Recall(GameObject unit)
        {
            // TODO Recall
            return isRecalling;
        }
    }
}