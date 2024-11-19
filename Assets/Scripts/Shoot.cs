using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : MonoBehaviour
{
    private static Shoot Instance { get; set; }
    [Header("Input Actions")]
    [SerializeField] private InputActionReference fire1;
    
    public float damage = 10f;

    [SerializeField] private Camera playerCamera;
    
    private void Awake()
    {
        Instance = this;
        fire1.action.Enable();
    }

    private void OnEnable()
    {
        fire1.action.Enable();
    }

    private void OnDisable()
    {
        fire1.action.Disable();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (fire1.action.triggered)
        {
            ShootFireOne();
        }
    }
    
    private void ShootFireOne()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit))
        {
            TargetHealth targetHealth = hit.transform.GetComponent<TargetHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
    }
}
