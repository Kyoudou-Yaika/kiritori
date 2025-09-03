using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    MeshCut2D.CutController MC;
    Vector2 StartPos, EndPos, Centerpos;

    // Start is called before the first frame update
    void Start()
    {
        MC = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartPos = new Vector2(transform.position.x, transform.position.y);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndPos = new Vector2(transform.position.x, transform.position.y);
            //Centerpos=StartPos-EndPos
            CC();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MC = collision.GetComponent<MeshCut2D.CutController>();
    }

    void CC()
    {
        MC.Startx = StartPos.x;
        MC.Starty = StartPos.y;
        MC.Endx = EndPos.x;
        MC.Endy = EndPos.y;
    }
}

