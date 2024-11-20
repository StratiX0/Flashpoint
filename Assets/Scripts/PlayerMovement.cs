using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement Instance { get; set; }

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference lookAction;
    
    [Header("Walk Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float groundDrag;
    private Vector3 _moveDirection;
    
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
    [SerializeField] private bool readyToJump;
    [SerializeField] private float airMultiplier;
    
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
    public Transform orientation;
    
    [Header("States")]
    public MovementState state;
    public enum MovementState
    {
        Grounded,
        Croutched,
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
        
        standYScale = transform.localScale.y;
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        _speedValue = Vector3.Magnitude(_rb.velocity);
        speedTextUi.text = _speedValue.ToString("F0");
        Movement();
        Jump();
        Look();
        StateHandler();
        SpeedControl();
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
        _moveDirection = orientation.forward * MoveVector.y + orientation.right * MoveVector.x;
        
        if (OnSlope() && !_exitSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * (movementSpeed * 20f), ForceMode.Force);

            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        else if (state == MovementState.Grounded)
        {
            _rb.AddForce(_moveDirection.normalized * (movementSpeed * 10f), ForceMode.Force); // Move the player on the ground
            _rb.drag = groundDrag;
        }
        else if (state == MovementState.Croutched)
        {
            _rb.AddForce(_moveDirection.normalized * (crouchSpeed * 10f), ForceMode.Force); // Move the player while crouching
            _rb.drag = groundDrag;
        }
        else
        {
            _rb.AddForce(_moveDirection.normalized * (movementSpeed * 10f * airMultiplier), ForceMode.Force); // Move the player in the air
            _rb.drag = 1;
        }
        
        _rb.useGravity = !OnSlope();
    }

    // Limit the player's speed
    private void SpeedControl()
    {
        if (OnSlope() && !_exitSlope)
        {
            if (_rb.velocity.magnitude > movementSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * movementSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // Get the velocity on the x and z axis
        
            if (flatVel.magnitude > movementSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * movementSpeed;
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
            Invoke(nameof(ResetJump), 0.5f);
        } 
    }
    
    // Reset the jump
    private void ResetJump()
    {
        readyToJump = true;
        _exitSlope = false;
    }

    private void StateHandler()
    {
        if (IsGrounded() && CrouchButton == 0f && CanCrouchUp())
        {
            state = MovementState.Grounded;
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            _applyCrouchingForce = false;
        }
        
        else if (IsGrounded() && CrouchButton == 0f && !CanCrouchUp())
        {
            state = MovementState.Croutched;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            _applyCrouchingForce = true;
        }
        
        else if (CrouchButton != 0f)
        {
            state = MovementState.Croutched;
            if (!_applyCrouchingForce && IsGrounded())
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
                _applyCrouchingForce = true;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            }
        }
        
        else
        {
            state = MovementState.Airborne;
        }
    }
    
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, transform.localScale.y * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
    }

    private bool CanCrouchUp()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, transform.localScale.x / 2f, Vector3.up, out hit, crouchYScale * 2f))
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
    private bool IsGrounded()
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
