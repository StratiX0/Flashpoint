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
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    private Vector3 _moveInput;
    private float _jumpInput;
    private float _crouchInput;
    private float _sprintInput;
    
    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    
    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;
    
    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;
    public PlayerCamera playerCam;
    public Transform playerCamTransform;
    
    private void Awake()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
        sprintAction.action.Enable();
    }
    
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
        sprintAction.action.Enable();
    }
    
    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        crouchAction.action.Disable();
        sprintAction.action.Disable();
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
        if (pm.wallRunning) WallRunningMovement();
    }

    private void CheckForWall() 
    {
        wallRight = Physics.Raycast(transform.position, playerCamTransform.transform.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -playerCamTransform.transform.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        _moveInput = moveAction.action.ReadValue<Vector2>();
        _jumpInput = jumpAction.action.ReadValue<float>();
        _crouchInput = crouchAction.action.ReadValue<float>();
        _sprintInput = sprintAction.action.ReadValue<float>();
    
        // State 1 - Wallrunning
        if((wallLeft || wallRight) && _moveInput.y > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallRunning)
                StartWallRun();
            
            // wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && pm.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            
            // wall jump
            if (_jumpInput != 0f) WallJump();
        }
        
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (pm.wallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
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

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // apply camera effects
        playerCam.DoWallRunFov(playerCam.baseFov * 1.1f);
        
        if (wallRight)
            playerCam.DoWallRunTilt(5f);
        else if (wallLeft)
            playerCam.DoWallRunTilt(-5f);
        
        // playerCam.DoWallRunTilt(5f);
    }
    
    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;
    
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
    
        if (useGravity) rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        
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
        
        playerCam.DoWallRunFov(playerCam.baseFov);
        playerCam.DoWallRunTilt(0f);
    }
    
    private void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;
    
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
    
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
    
        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
