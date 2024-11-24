using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    public TargetHealth Instance { get; set; }
    
    [Header("Health Settings")]
    public float defaultHealth;
    private float _health;

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
        
        _health = defaultHealth;
    }
    
    // Reduces health to give damage
    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0f)
        {
            _health = 0f;
            SetMaterial(destroyedMat);
        }
    }
    
    // Set health to a specific value
    public void SetHealth(float value)
    {
        if (value >= 0f) SetMaterial(defaultMat);

        if (value <= 0f) value = 0f;
        _health = value;
    }
    
    // Returns the current health value
    public float GetHealth()
    {
        return _health;
    }
    
    // Increases health by a specific amount
    public void Heal(float healAmount)
    {
        _health += healAmount;
    }

    // Sets the material of the object
    public void SetMaterial(Material mat)
    {
        Instance.gameObject.GetComponent<Renderer>().material = mat;
    }
}
