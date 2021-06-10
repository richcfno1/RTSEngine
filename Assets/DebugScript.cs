using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public Animator test;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            test.SetBool("IsMoving", true);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            test.SetBool("IsMoving", false);
        }
    }
}
