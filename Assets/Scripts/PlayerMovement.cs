using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement Instance { get; set; }

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference lookAction;
    
    [Header("Walk Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float movementSpeed;
    
    [Header("Sprint Settings")]
    [SerializeField] private float sprintSpeed;
    public float MovementSpeed => movementSpeed;
    [SerializeField] private float groundDrag;
    private Vector3 _moveDirection;
    
    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideForce;
    [SerializeField] private float minimumSlideSpeed;
    public float MinimumSlideSpeed => slideSpeed;
    public bool sliding;
    private float _desiredMoveSpeed;
    private float _lastDesireMoveSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    
    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed;
    private bool _applyCrouchingForce;
    [SerializeField] private float crouchYScale;
    [SerializeField] private float standYScale;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitSlope;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private bool readyToJump;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float jumpCooldown;
    
    [Header("Physics Settings")]
    [SerializeField] private float gravityScale;
    private bool _isGrounded;
    private float _speedValue;
    
    [Header("Mouse Settings")]
    [SerializeField] private float sensitivity;
    private float _cameraPitch;
    
    [Header("User Interface")]
    [SerializeField] private TextMeshProUGUI speedTextUi;

    private Vector2 MoveVector { get; set; }
    private float JumpButton { get; set; }
    private float CrouchButton { get; set; }
    private Vector2 MouseDirection { get; set; }
    
    private Rigidbody _rb;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    public Transform cameraOrientation;
    
    [Header("States")]
    public MovementState state;
    public enum MovementState
    {
        Grounded,
        Croutched,
        Sprinting,
        Sliding,
        Airborne
    }
    
    
    private void Awake()
    {
        Instance = this;
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        crouchAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        
        readyToJump = true;
        
        standYScale = transform.localScale.y;
    }

    private void Update()
    {
        GetInput();
        Look();
        StateHandler();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        _speedValue = Vector3.Magnitude(_rb.velocity);
        speedTextUi.text = _speedValue.ToString("F0");
        Movement();
        Jump();
        _rb.AddForce(Vector3.down * gravityScale, ForceMode.Force);
    }

    private void GetInput()
    {
        MoveVector = moveAction.action.ReadValue<Vector2>();
        JumpButton = jumpAction.action.ReadValue<float>();
        CrouchButton = crouchAction.action.ReadValue<float>();
        MouseDirection = lookAction.action.ReadValue<Vector2>();
    }

    // Move the player
    private void Movement()
    {
        _moveDirection = cameraOrientation.forward * MoveVector.y + cameraOrientation.right * MoveVector.x;
        
        if (OnSlope() && !_exitSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection(_moveDirection) * (_desiredMoveSpeed * 20f), ForceMode.Force);

            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        else if (state == MovementState.Grounded)
        {
            _rb.AddForce(_moveDirection.normalized * (_desiredMoveSpeed * 10f), ForceMode.Force); // Move the player on the ground
            _rb.drag = groundDrag;
        }
        else if (state == MovementState.Sliding)
        {
            SlidingMovement();
            _rb.drag = groundDrag;
        }
        else if (state == MovementState.Croutched)
        {
            _rb.AddForce(_moveDirection.normalized * (_desiredMoveSpeed * 10f), ForceMode.Force); // Move the player while crouching
            _rb.drag = groundDrag;
        }
        else
        {
            _rb.AddForce(_moveDirection.normalized * (_desiredMoveSpeed * _speedValue), ForceMode.Force); // Move the player in the air
            _rb.drag = 1;
        }
        
        _rb.useGravity = !OnSlope();
    }
    
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - movementSpeed);
        float startValue = movementSpeed;

        while (time < difference)
        {
            movementSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        movementSpeed = _desiredMoveSpeed;
    }

    // Limit the player's speed
    private void SpeedControl()
    {
        if (OnSlope() && !_exitSlope)
        {
            if (_rb.velocity.magnitude > _desiredMoveSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * _desiredMoveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // Get the velocity on the x and z axis
        
            if (flatVel.magnitude > _desiredMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * _desiredMoveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
    }

    // Make the player jump
    private void Jump()
    {
        _exitSlope = true;
        
        if (JumpButton != 0 && IsGrounded() && readyToJump)
        {
            readyToJump = false;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Jump
            Invoke(nameof(ResetJump), jumpCooldown);
        } 
    }
    
    // Reset the jump
    private void ResetJump()
    {
        readyToJump = true;
        _exitSlope = false;
    }
    
    private void StartSlide()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
    }
    
    private void SlidingMovement()
    {
        _moveDirection = cameraOrientation.forward * MoveVector.y + cameraOrientation.right * MoveVector.x;
        
        if (!OnSlope() || _rb.velocity.y > 0.1f)
        {
            _rb.AddForce(_moveDirection.normalized * slideForce, ForceMode.Force);
        }
        else
        {
            _rb.AddForce(GetSlopeMoveDirection(_moveDirection) * slideForce, ForceMode.Force);
        }
        
        if (movementSpeed <= minimumSlideSpeed)
        {
            StopSlide();
        }
    }
    
    private void StopSlide()
    {
        if (IsGrounded())
        {
            state = MovementState.Grounded;
        }
        
        transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
    }

    // Manage the player's state
    private void StateHandler()
    {
        if (CrouchButton != 0f && _speedValue > minimumSlideSpeed && IsGrounded()) // Slide
        {
            state = MovementState.Sliding; 
            _desiredMoveSpeed = slideSpeed;
            
            if (!_applyCrouchingForce && IsGrounded())
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
                _applyCrouchingForce = true;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            }
            
            StartSlide();
            
            if (OnSlope() && _rb.velocity.y < 0.1f)
            {
                _desiredMoveSpeed = slideSpeed;
            }
        }
        
        else if (CrouchButton != 0f && (_speedValue <= crouchSpeed && IsGrounded() || !IsGrounded())) // Crouch
        {
            state = MovementState.Croutched;
            if (IsGrounded()) _desiredMoveSpeed = crouchSpeed;
            if (!_applyCrouchingForce && IsGrounded())
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
                _applyCrouchingForce = true;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            }
        }
        
        else if (IsGrounded() && CrouchButton == 0f && CanCrouchUp()) // Stand
        {
            state = MovementState.Grounded;
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            _applyCrouchingForce = false;
            _desiredMoveSpeed = walkSpeed;
        }
        
        else if (IsGrounded() && CrouchButton == 0f && !CanCrouchUp()) // Crouch if under an object you cannot stand under
        {
            state = MovementState.Croutched;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            _applyCrouchingForce = true;
            _desiredMoveSpeed = crouchSpeed;
        }
        
        else if (!IsGrounded() && CrouchButton == 0f) // Airborne
        {
            state = MovementState.Airborne;
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            _applyCrouchingForce = false;
            _desiredMoveSpeed = jumpSpeed;
        }
        
        if (Mathf.Abs(_desiredMoveSpeed - _lastDesireMoveSpeed) > 4f && movementSpeed != 0f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            movementSpeed = _desiredMoveSpeed;
        }
        
        _lastDesireMoveSpeed = _desiredMoveSpeed;
    }
    
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, transform.localScale.y * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }

    private bool CanCrouchUp()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, crouchYScale * 2f))
        {
            return false;
        }

        return true;
    }

    // Look around
    private void Look()
    {
        float mouseX = MouseDirection.x * sensitivity;
        float mouseY = MouseDirection.y * sensitivity;

        transform.Rotate(Vector3.up, mouseX * Time.deltaTime); // Rotate the player on the yaw axis

        _cameraPitch -= mouseY * Time.deltaTime;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f); // Rotate the camera on the pitch axis
    }

    // Check if the player is grounded
    public bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
        
        return _isGrounded;
    }


}
