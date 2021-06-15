using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierSubsystemBaseScript : SubsystemBaseScript
{
    public List<string> products;
    public int carrierVolume;
    public List<Vector3> deployPath;
    public List<Vector3> retrievePath;

    [HideInInspector]
    public List<GameObject> deployedUnits = new List<GameObject>();
    public Dictionary<string, int> carriedUnits = new Dictionary<string, int>();
    private int numberOfUnitInProducing = 0;

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
        int tempCount = 0;
        foreach (KeyValuePair<string, int> i in carriedUnits)
        {
            tempCount += i.Value;
        }
        if (tempCount + numberOfUnitInProducing + deployedUnits.Count >= carrierVolume)
        {
            return;
        }
        if (products.Contains(type) && GameManager.GameManagerInstance.unitLibrary.ContainsKey(type))
        {
            string baseTypeName = GameManager.GameManagerInstance.unitLibrary[type].baseTypeName;
            numberOfUnitInProducing++;
            StartCoroutine(FinishProducingAfter(type, Resources.Load<GameObject>(
                GameManager.GameManagerInstance.gameObjectLibrary[baseTypeName]).GetComponent<UnitBaseScript>().buildTime));
        }
    }
    private IEnumerator FinishProducingAfter(string type, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        carriedUnits[type]++;
        numberOfUnitInProducing--;
    }

    public virtual void Deploy(string type)
    {
        if (carriedUnits.ContainsKey(type))
        {
            if (carriedUnits[type] > 0)
            {
                GameObject temp = GameManager.GameManagerInstance.InstantiateUnit(type, 
                    transform.TransformPoint(transform.localPosition + new Vector3(0, 10, 0)), 
                    transform.rotation, GameObject.Find("GameObject").transform, BelongTo);
                carriedUnits[type]--;
                deployedUnits.Add(temp);
            }
        }
    }

    public virtual void Retrieve(GameObject unit)
    {

    }
}
