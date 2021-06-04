using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectBaseScript : MonoBehaviour
{
    public string typeID;
    public float maxHP;

    public int Index { get; set; }
    public int BelongTo { get; set; }
    public float HP { get; set; }

    private GameObject lastDamagedBy = null;

    // Start is called before the first frame update
    void Start()
    {
        HP = maxHP;
        GameManager.GameManagerInstance.OnGameObjectCreated(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        GameManager.GameManagerInstance.OnGameObjectDestroyed(gameObject, lastDamagedBy);
    }

    public virtual void Damage()
    {

    }
}