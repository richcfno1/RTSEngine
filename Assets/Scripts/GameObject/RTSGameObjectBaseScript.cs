using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSGameObjectBaseScript : MonoBehaviour
{
    // Set by editor
    public string typeID;
    public float maxHP;
    public GameObject onDestroyedEffect;

    // Set when instantiate
    public int Index { get; set; }
    public int BelongTo { get; set; }
    public float HP { get; set; }

    protected GameObject lastDamagedBy = null;

    protected virtual void OnCreatedAction()
    {
        HP = maxHP;
        GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
    }

    protected virtual void OnDestroyedAction()
    {
        GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, lastDamagedBy);
        Destroy(gameObject);
        Instantiate(onDestroyedEffect, transform.position, new Quaternion());
    }

    public virtual void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float movePowerReduce, GameObject from)
    {
        HP = Mathf.Clamp(HP - damage, 0, maxHP);
        lastDamagedBy = from;
    }
}