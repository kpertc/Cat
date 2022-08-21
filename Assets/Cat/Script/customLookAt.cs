using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class customLookAt : MonoBehaviour
{
    public Transform target;

    private Vector3 target_initPos;

    private void Awake()
    {
        target_initPos = target.position;
    }

    void Update()
    {
        Vector3 lookPos = target.position - transform.position;
        
        lookPos.y = target_initPos.y;
        lookPos.z = target_initPos.z;
        
        Quaternion rotation = Quaternion.LookRotation(lookPos);

        transform.rotation = rotation;
    }
}
