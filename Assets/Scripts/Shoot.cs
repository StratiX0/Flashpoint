using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : MonoBehaviour
{
    private static Shoot Instance { get; set; }
    [Header("Input Actions")]
    [SerializeField] private InputActionReference fire1;
    [SerializeField] private InputActionReference fire2;
    private bool _fireOneState;
    private float _fireTwoState;
    
    public float primaryDamage = 10f;
    public float secondaryDamage = 1.2f;

    [SerializeField] private Camera playerCamera;
    
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
        if (_fireOneState)
        {
            ShootFireOne();
        }
        
        if (_fireTwoState != 0f)
        {
            ShootFireTwo();
        }
    }
    
    // Get the input from the player
    private void GetInput()
    {
        _fireOneState = fire1.action.triggered;
        _fireTwoState = fire2.action.ReadValue<float>();
    }
    
    // Shoots a single bullet on click
    private void ShootFireOne()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit))
        {
            TargetHealth targetHealth = hit.transform.GetComponent<TargetHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(primaryDamage);
            }
        }
    }
    
    // Shoots a continuous beam
    private void ShootFireTwo()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit))
        {
            TargetHealth targetHealth = hit.transform.GetComponent<TargetHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(secondaryDamage * Time.deltaTime);
            }
        }
    }
}
