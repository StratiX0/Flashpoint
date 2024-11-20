using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Target : MonoBehaviour
{
    private Target Instance { get; set; }
    private TargetHealth _targetHealth;
    
    [Header("Target Settings")]
    [SerializeField] private Type targetType;
    
    [Header("Materials")]
    [SerializeField] private Material tapMat;
    [SerializeField] private Material laserMat;
    [SerializeField] private Material dualMat;
    
    public enum Type
    {
        Tap,
        Laser,
        Dual
    }
    
    void Awake()
    {
        Instance = this;
        
        _targetHealth = gameObject.GetComponent<TargetHealth>();
        if (targetType == Type.Tap)
        {
            _targetHealth.defaultMat = tapMat;
        }
        else if (targetType == Type.Laser)
        {
            _targetHealth.defaultMat = laserMat;
        }
        else if (targetType == Type.Dual)
        {
            _targetHealth.defaultMat = dualMat;
        }
    }
    
    public Type GetTargetType()
    {
        return targetType;
    }

    private void Start()
    {
        
    }
}
