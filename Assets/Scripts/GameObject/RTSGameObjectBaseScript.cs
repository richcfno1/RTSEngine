using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSGameObjectBaseScript : MonoBehaviour
{
    // Set by editor
    public string typeID;
    public float maxHP;

    // Set when instantiate
    public int Index { get; set; }
    public int BelongTo { get; set; }
    public float HP { get; set; }

    protected GameObject lastDamagedBy = null;

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
    }

    protected virtual void OnCreatedAction()
    {
        HP = maxHP;
        GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
    }

    protected virtual void OnDestroyedAction()
    {
        GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, lastDamagedBy);
        Destroy(gameObject);
    }

    public virtual void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
    {
        HP = Mathf.Clamp(HP - damage, 0, maxHP);
        lastDamagedBy = from;
    }
}