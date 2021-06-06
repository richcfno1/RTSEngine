using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBaseScript : MonoBehaviour
{
    public float damage;
    public float moveSpeed;
    public float maxTime;
    [HideInInspector]
    public Vector3 moveDirection;
    [HideInInspector]
    public List<Collider> toIgnore = new List<Collider>();
    [HideInInspector]
    public GameObject createdBy;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += moveDirection * Time.fixedDeltaTime *  moveSpeed;
        timer += Time.fixedDeltaTime;
        if (timer > maxTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (toIgnore.Contains(other))
        {
            return;
        }
        // If there is a subsystem gameobject in front of the bullet, hit it instead of the ship
        RaycastHit hit;
        if (Physics.Raycast(other.ClosestPoint(transform.position), moveDirection, out hit, other.bounds.size.sqrMagnitude))
        {
            if (hit.collider != other && hit.collider.GetComponent<SubsystemBaseScript>() != null)
            {
                hit.collider.GetComponent<SubsystemBaseScript>().CreateDamage(damage, createdBy);
                Destroy(gameObject);
                return;
            }
        }
        if (other.GetComponent<GameObjectBaseScript>() != null)
        {
            other.GetComponent<GameObjectBaseScript>().CreateDamage(damage, createdBy);
        }
        // Or it may be a aim collider, we need to find its parents
        else if (other.tag == "AimCollider" && other.GetComponentInParent<GameObjectBaseScript>() != null)
        {
            other.GetComponentInParent<GameObjectBaseScript>().CreateDamage(damage, createdBy);
        }
        // Well... in fact, there is still a case that bullet collide with bullet...
        Destroy(gameObject);
    }
}
