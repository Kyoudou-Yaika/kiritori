using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterNewObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // ê∂ê¨ÇµÇΩObjectÇÃå≈íËç¿ïW
        transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0f);
        transform.rotation = new Quaternion(0f, 0f, gameObject.transform.rotation.z, 1f);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
