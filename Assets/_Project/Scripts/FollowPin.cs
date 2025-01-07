using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPin : MonoBehaviour
{
    public float height = 0.5f;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        line.SetPosition(0, transform.parent.position);
        line.SetPosition(1, transform.parent.position + new Vector3(0, height, 0));
    }
}
