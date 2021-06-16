using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierSubsystemBaseScript : SubsystemBaseScript
{
    public List<string> products;
    public int carrierVolume;
    public float deployTime;
    public List<Vector3> deployPath;
    public List<Vector3> retrievePath;

    [HideInInspector]
    public List<GameObject> deployedUnits = new List<GameObject>();
    public Dictionary<string, int> carriedUnits = new Dictionary<string, int>();

    private Queue<string> produceQueue = new Queue<string>();
    private Queue<string> deployQueue = new Queue<string>();

    private bool isProducing = false;
    private bool isDeploying = false;

    // Start is called before the first frame update
    void Start()
    {
        OnCreatedAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP <= 0)
        {
            OnDestroyedAction();
        }
        if (!Active && HP / maxHP > repairPercentRequired)
        {
            OnSubsystemRepairedAction();
        }
        deployedUnits.RemoveAll(x => x == null);

        if (!isProducing && produceQueue.Count != 0)
        {
            string unitType = produceQueue.Peek();
            string baseTypeName = GameManager.GameManagerInstance.unitLibrary[unitType].baseTypeName;
            isProducing = true;
            StartCoroutine(FinishProducingAfter(unitType, Resources.Load<GameObject>(
                GameManager.GameManagerInstance.gameObjectLibrary[baseTypeName]).GetComponent<UnitBaseScript>().buildTime));
        }
        if (!isDeploying && deployQueue.Count != 0)
        {
            string unitType = deployQueue.Peek();
            isDeploying = true;
            StartCoroutine(FinishDeployingAfter(unitType, deployTime));
        }
    }

    private IEnumerator FinishProducingAfter(string type, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        produceQueue.Dequeue();
        carriedUnits[type]++;
        isProducing = false;
    }

    private IEnumerator FinishDeployingAfter(string type, float waitTime)
    {
        GameObject temp = GameManager.GameManagerInstance.InstantiateUnit(type,
            transform.TransformPoint(transform.localPosition),
            transform.rotation, GameObject.Find("GameObject").transform, BelongTo);
        List<Vector3> trueDeployPath = new List<Vector3>();
        foreach (Vector3 i in deployPath)
        {
            trueDeployPath.Add(transform.position + i);
        }
        temp.GetComponent<UnitBaseScript>().ForcedMove(trueDeployPath);
        carriedUnits[type]--;
        deployedUnits.Add(temp);
        deployQueue.Dequeue();
        yield return new WaitForSeconds(waitTime);
        isDeploying = false;
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
        }
    }

    protected override void OnDestroyedAction()
    {
        base.OnDestroyedAction();
        StopAllCoroutines();
    }

    public virtual void Produce(string type)
    {
        int carriedCount = 0;
        foreach (KeyValuePair<string, int> i in carriedUnits)
        {
            carriedCount += i.Value;
        }
        if (carriedCount + produceQueue.Count + deployedUnits.Count >= carrierVolume)
        {
            return;
        }
        if (products.Contains(type) && GameManager.GameManagerInstance.unitLibrary.ContainsKey(type))
        {
            produceQueue.Enqueue(type);
        }
    }

    public virtual void Deploy(string type)
    {
        if (carriedUnits.ContainsKey(type))
        {
            if (carriedUnits[type] > 0)
            {
                deployQueue.Enqueue(type);
            }
        }
    }

    public virtual void Retrieve(GameObject unit)
    {

    }
}
