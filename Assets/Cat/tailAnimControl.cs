using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class tailAnimControl : MonoBehaviour
{
    public Animator _animator;

    public string currentPlaying;
    public string[] clipname;

    public float animSwithTimeMax = 5;
    public float animSwithTimeMin = 2;
    
    public float animSwitchTime;
    
    [Space(20)]
    
    public float minSpeed = 0.01f;
    public float maxSpeed = 0.1f;
    public float minAngle = -80f;
    public float maxAngle = 80f;
    
    [Space(20)]

    [SerializeField]private float playSpeed;
    [SerializeField] private float currentAngle;
    [SerializeField]private float timeCount = 0.0f;

    private Quaternion beginRotation;
    private Quaternion endRotation;

    private void Awake()
    {
        setRandomRotation();

        playRandomClip();
    }
    void Update()
    {
        // Rotation
        // when complete
        if (timeCount > 1) setRandomRotation();
        
        timeCount += Time.deltaTime * playSpeed;
        transform.rotation = Quaternion.Lerp(beginRotation, endRotation, timeCount);
        
        //animator
        //Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).length);
        //_animator.GetCurrentAnimatorClipInfoCount()
        //_animator.GetCurrentAnimatorStateInfo(0).

        animSwitchTime -= Time.deltaTime;
        
        if (animSwitchTime < 0) playRandomClip();
        
    }

    void setRandomRotation()
    {
        timeCount = 0;
        beginRotation = endRotation;

        currentAngle = Random.Range(minAngle, maxAngle);
        endRotation = Quaternion.Euler(0, 0, currentAngle);
        playSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void playRandomClip()
    {
        int number = (int)Random.Range(0, clipname.Length);
        _animator.SetTrigger(clipname[number]);
        currentPlaying = clipname[number];

        animSwitchTime = Random.Range(animSwithTimeMin,animSwithTimeMax);
    }
    
    [ContextMenu("p1")] void p1() =>  _animator.SetTrigger("p1"); 
    
    [ContextMenu("p1r")] void p1r() =>  _animator.SetTrigger("p1r");  
    
    [ContextMenu("p2")] void p2() =>  _animator.SetTrigger("p2"); 
    
    [ContextMenu("p3")] void p3() =>  _animator.SetTrigger("p3");  
}
