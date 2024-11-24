using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    private Vector3 _moveInput;
    private float _crouchInput;
    private float _sprintInput;
    
    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void Awake()
    {
        moveAction.action.Enable();
    }
    
    private void OnEnable()
    {
        moveAction.action.Enable();
    }
    
    private void OnDisable()
    {
        moveAction.action.Disable();
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallRunning)
            WallRunningMovement();
    }

    private void CheckForWall()
{
    wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
    wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
}

private bool AboveGround()
{
    return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
}

private void StateMachine()
{
    _moveInput = moveAction.action.ReadValue<Vector2>();
    _crouchInput = crouchAction.action.ReadValue<float>();
    _sprintInput = sprintAction.action.ReadValue<float>();

    // State 1 - Wallrunning
    if((wallLeft || wallRight) && _moveInput.y > 0 && AboveGround())
    {
        if (!pm.wallRunning)
            StartWallRun();
    }
    // State 3 - None
    else
    {
        if (pm.wallRunning)
            StopWallRun();
    }
}

private void StartWallRun()
{
    pm.wallRunning = true;
    wallRunTimer = maxWallRunTime; // Reset the wall run timer
}

private void WallRunningMovement()
{
    rb.useGravity = false;
    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
    Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

    if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        wallForward = -wallForward;

    // forward force
    rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

    // upwards/downwards force
    if (_sprintInput > 0f)
        rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
    if (_crouchInput > 0f)
        rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

    // push to wall force
    if (!(wallLeft && _moveInput.x > 0) && !(wallRight && _moveInput.x < 0))
        rb.AddForce(-wallNormal * 100, ForceMode.Force);

    // Decrease the wall run timer
    wallRunTimer -= Time.deltaTime;
    if (wallRunTimer <= 0)
    {
        StopWallRun();
    }
}

private void StopWallRun()
{
    pm.wallRunning = false;
    rb.useGravity = true; // Re-enable gravity when wall running stops
}
}
