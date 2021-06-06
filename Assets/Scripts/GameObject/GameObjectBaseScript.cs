using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectBaseScript : MonoBehaviour
{
    // Set by editor
    public string typeID;
    public float maxHP;

    // Set when instantiate
    public int Index { get; set; }
    public int BelongTo { get; set; }
    public float HP { get; set; }

    private GameObject lastDamagedBy = null;

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

    public virtual void CreateDamage(float amount, GameObject from)
    {
        HP = Mathf.Clamp(HP - amount, 0, maxHP);
        lastDamagedBy = from;
    }
}