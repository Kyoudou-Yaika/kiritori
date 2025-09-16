using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterNewObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
