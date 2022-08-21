using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseControl : MonoBehaviour
{
    public float maxDistance = 0.1f;
    public float moveScale = 0.01f;

    public bool isZAxis = true;

    [Space(10)]

    [SerializeField] private Vector3 initPos;
    [SerializeField] private Vector2 mousePosDiff;
    [SerializeField] private Vector2 lastMousePos;

    private void Awake()
    {
        initPos = transform.position;
    }

    //void Start()
    //{
        
    //}

    void Update()
    {
        mousePosDiff = lastMousePos - (Vector2)Input.mousePosition;

        //if (mousePosDiff == lastMousePos) Debug.Log("first frame");


            // output different direction

            float x = Mathf.Clamp(
                transform.position.x + mousePosDiff.x * - moveScale,
                initPos.x - maxDistance,
                initPos.x + maxDistance
                );
        
            float y = initPos.y;

            // Z 
            if (isZAxis)
            {
                float z = Mathf.Clamp(
                    transform.position.z + mousePosDiff.y * - moveScale,
                    initPos.z - maxDistance,
                    initPos.z + maxDistance
                );
                
                transform.position = new Vector3(x, y, z);
            }
            
            else transform.position = new Vector3(x, y, initPos.z);
        

        lastMousePos = Input.mousePosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3( maxDistance * 2 , maxDistance, maxDistance * 2));
    }
}
