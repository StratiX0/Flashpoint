using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement Instance { get; set; }

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference lookAction;
    
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool readyToJump;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float gravityScale;
    
    [Header("Mouse Settings")]
    [SerializeField] private float sensitivity;
    private float _cameraPitch;

    private Vector2 MoveVector { get; set; }
    private float JumpButton { get; set; }
    private Vector2 MouseDirection { get; set; }
    
    private Rigidbody _rb;
    [SerializeField] private Camera playerCamera;
    
    public Transform orientation;
    
    
    private void Awake()
    {
        Instance = this;
        moveAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        GetInput();
        Look();
        Jump();
    }

    private void FixedUpdate()
    {
        Movement();
        SpeedControl();
        _rb.AddForce(Vector3.down * gravityScale, ForceMode.Force);
    }

    private void GetInput()
    {
        MoveVector = moveAction.action.ReadValue<Vector2>();
        JumpButton = jumpAction.action.ReadValue<float>();
        MouseDirection = lookAction.action.ReadValue<Vector2>();
    }

    // Move the player
    private void Movement()
    {
        Vector3 moveDirection = orientation.forward * MoveVector.y + orientation.right * MoveVector.x;
        
        if (IsGrounded())
        {
            _rb.AddForce(moveDirection.normalized * (speed * 10f), ForceMode.Force); // Move the player on the ground
            _rb.drag = groundDrag;
        }
        else
        {
            _rb.AddForce(moveDirection.normalized * (speed * 10f * airMultiplier), ForceMode.Force); // Move the player in the air
            _rb.drag = 1;
        }
    }

    // Limit the player's speed
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // Get the velocity on the x and z axis
        
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    // Make the player jump
    private void Jump()
    {
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
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }


}
