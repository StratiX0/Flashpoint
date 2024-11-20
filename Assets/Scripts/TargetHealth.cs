using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    private TargetHealth Instance { get; set; }
    
    [Header("Health Settings")]
    [SerializeField] private float health;

    [Header("Materials")]
    public Material defaultMat;
    public Material destroyedMat;
    private Material _currentMat;
    
    private void Awake()
    {
        Instance = this;
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
