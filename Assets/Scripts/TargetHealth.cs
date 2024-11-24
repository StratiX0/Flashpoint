using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    public TargetHealth Instance { get; set; }
    
    [Header("Health Settings")]
    [SerializeField] private float health;

    [Header("Target Settings")]
    public Type targetType;
    
    [Header("Materials")]
    public Material defaultMat;
    [SerializeField] private Material destroyedMat;
    [SerializeField] private Material tapMat;
    [SerializeField] private Material laserMat;
    [SerializeField] private Material dualMat;
    
    public enum Type
    {
        Tap,
        Laser,
        Dual
    }
    
    private void Awake()
    {
        Instance = this;
        
        destroyedMat = Resources.Load<Material>("Target Materials/Target Destroyed");
        tapMat = Resources.Load<Material>("Target Materials/Target Tap");
        laserMat = Resources.Load<Material>("Target Materials/Target Laser");
        dualMat = Resources.Load<Material>("Target Materials/Target Dual");
        
        if (targetType == Type.Tap)
        {
            defaultMat = tapMat;
        }
        else if (targetType == Type.Laser)
        {
            defaultMat = laserMat;
        }
        else if (targetType == Type.Dual)
        {
            defaultMat = dualMat;
        }
        
        SetMaterial(defaultMat);
    }
    
    // Reduces health to give damage
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            health = 0f;
            SetMaterial(destroyedMat);
        }
    }
    
    // Set health to a specific value
    public void SetHealth(float value)
    {
        if (value >= 0f) SetMaterial(defaultMat);

        if (value <= 0f) value = 0f;
        health = value;
    }
    
    // Returns the current health value
    public float GetHealth()
    {
        return health;
    }
    
    // Increases health by a specific amount
    public void Heal(float healAmount)
    {
        health += healAmount;
    }

    // Sets the material of the object
    public void SetMaterial(Material mat)
    {
        Instance.gameObject.GetComponent<Renderer>().material = mat;
    }
}
