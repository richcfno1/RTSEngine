using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBaseScript : MonoBehaviour
{
    public float moveSpeed;
    public Vector3 moveDirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * Time.deltaTime *  moveSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        // If there is a subsystem gameobject in front of the bullet, hit it instead of the hull
        RaycastHit hit;
        if (Physics.Raycast(other.ClosestPoint(transform.position), moveDirection, out hit, other.bounds.size.sqrMagnitude))
        {
            if (hit.collider != other && hit.collider.GetComponent<SubsystemBaseScript>() != null)
            {
                Debug.Log("Hit subsystem: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("Hit hull: " + other.name);
        }
        Destroy(gameObject);
    }
}
