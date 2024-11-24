using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Shoot : MonoBehaviour
{
    private static Shoot Instance { get; set; }
    
    [SerializeField] private Camera playerCamera;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference fire1;
    [SerializeField] private InputActionReference fire2;
    private bool _fireOneState;
    private float _fireTwoState;
    
    [Header("Weapon Settings")]
    public float primaryDamage;
    public float secondaryDamage;
    public float impactForceOne;
    public float impactForceTwo;
    
    [Header("VFX")]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private GameObject muzzleLight;
    [SerializeField] private float muzzleLightTime;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject impactEffectFolder;
    
    private void Awake()
    {
        Instance = this;
        fire1.action.Enable();
        fire2.action.Enable();
    }

    private void OnEnable()
    {
        fire1.action.Enable();
        fire2.action.Enable();
    }

    private void OnDisable()
    {
        fire1.action.Disable();
        fire2.action.Disable();
    }
    
    // Update is called once per frame
    void Update()
    {
        GetInput();
        ManageShoot();
    }
    
    // Get the input from the player
    private void GetInput()
    {
        _fireOneState = fire1.action.triggered;
        _fireTwoState = fire2.action.ReadValue<float>();
    }

    // Manages the shooting of the player
    private void ManageShoot()
    {
        if (_fireOneState) // If the player is shooting with the primary shoot method
        {
            ShootFireOne();
        }
        
        if (_fireTwoState != 0f) // If the player is shooting with the secondary shoot method
        {
            ShootFireTwo();
        }
    }
    
    // Shoots a single bullet on click
    private void ShootFireOne()
    {
        // Plays the muzzle flash and sets the muzzle light to active
        muzzleFlash.Play();
        muzzleLight.SetActive(true);
        
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit)) // Shoots a raycast from the camera
        {
            TargetHealth targetHealth = hit.transform.GetComponent<TargetHealth>();
            TargetHealth.Type type = targetHealth.targetType;
            if (targetHealth != null && (type == TargetHealth.Type.Tap || type == TargetHealth.Type.Dual))
            {
                targetHealth.TakeDamage(primaryDamage); // Deals damage to the target
            }
            
            ShootForce(hit, impactForceOne); // Applies a force to the object that has been hit
            
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal), impactEffectFolder.transform); // Creates an impact effect on what has been hit
        }
        
        Invoke(nameof(ResetMuzzleLight), muzzleLightTime); // Resets the muzzle light after a certain amount of time
    }
    
    // Resets the muzzle light
    private void ResetMuzzleLight()
    {
        muzzleLight.SetActive(false);
    }
    
    // Shoots a continuous beam
    private void ShootFireTwo()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit)) // Shoots a raycast from the camera
        {
            TargetHealth targetHealth = hit.transform.GetComponent<TargetHealth>();
            TargetHealth.Type type = targetHealth.targetType;
            if (targetHealth != null && (type == TargetHealth.Type.Laser || type == TargetHealth.Type.Dual))
            {
                targetHealth.TakeDamage(secondaryDamage * Time.deltaTime); // Deals damage to the target
            }
            
            ShootForce(hit, impactForceTwo); // Applies a force to the object that has been hit
        }
    }

    // Applies a force to the object that has been hit
    private void ShootForce(RaycastHit hit, float impactForce)
    {
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Force);
        }
    }
}
