using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("_moveSpeed")] [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float speedVelocity;
    public float speedFlatVelocity;
    
    public float desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    
    public float groundDrag;

    [Header("Curves")]
    public AnimationCurve speedCurve;
    public AnimationCurve slideCurve;
    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float standYScale;
    public bool crouchForceApplied;

    [Header("Sliding")]
    public float slideSpeed;
    public bool sliding;
    private bool _slideOccured;
    private float _updatedSlideSpeed;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    
    private Vector3 _moveInput;
    private float _jumpInput;
    private float _crouchInput;
    private float _sprintInput;
        
    [Header("Ground Check")]
    public float playerHeight;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public Transform orientation;

    public Vector3 moveDirection;

    private Rigidbody _rb;

    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        Air
    }

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
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        readyToJump = true;

        standYScale = transform.localScale.y;
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {
        Vector3 vel = _rb.velocity;
        speedVelocity = vel.magnitude;
        vel.y = 0;
        speedFlatVelocity = vel.magnitude;
        MovePlayer();
    }

    private void MyInput()
    {
        _moveInput = moveAction.action.ReadValue<Vector2>();
        _jumpInput = jumpAction.action.ReadValue<float>();
        _crouchInput = crouchAction.action.ReadValue<float>();
        _sprintInput = sprintAction.action.ReadValue<float>();
        
        // when to jump
        if(_jumpInput != 0f && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (_crouchInput != 0f)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            if (!crouchForceApplied)
            {
                _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                crouchForceApplied = true;
            }
            
            if (!_slideOccured)
            {
                _updatedSlideSpeed = slideSpeed;    
                _slideOccured = true;
            }
        }

        // stop crouch
        if (_crouchInput == 0f)
        {
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            crouchForceApplied = false;
            _slideOccured = false;
        }
    }

    private void StateHandler()
    {
        // Mode - Sliding
        if (sliding)
        {
            state = MovementState.Sliding;
            
            if (OnSlope() && _rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = _updatedSlideSpeed;
            }
        }
        
        // Mode - Crouching
        else if (_crouchInput != 0f && !sliding)
        {
            if (state != MovementState.Air) desiredMoveSpeed = crouchSpeed;
            state = MovementState.Crouching;
        }

        // Mode - Sprinting
        else if(grounded && _sprintInput > 0f)
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.Walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.Air;
        }
        
        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - _lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        
        _lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0f;
        float duration = speedCurve.keys[speedCurve.length - 1].time;
        float startValue = moveSpeed;

        while (time < duration)
        {
            float t = time / duration;
            if (sliding)
            {
                desiredMoveSpeed = Mathf.Lerp(crouchSpeed, desiredMoveSpeed, slideCurve.Evaluate(t));

                moveSpeed = desiredMoveSpeed;
                
                if (moveSpeed <= crouchSpeed)
                {
                    _updatedSlideSpeed = crouchSpeed;
                    _slideOccured = true;
                    sliding = false;
                    state = MovementState.Crouching;
                    break;
                }
            }
            else
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, speedCurve.Evaluate(t));
            }

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else if (sliding)
            {
                time += Time.deltaTime * 0.5f;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;

        // on slope
        if (OnSlope() && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);

            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
        {
            _rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        }

        // in air
        else if (!grounded)
        {
            _rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
        }

        // turn gravity off while on slope
        _rb.useGravity = !OnSlope();
        
        orientation.localPosition = transform.localPosition;
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !_exitingSlope)
        {
            if (_rb.velocity.magnitude > moveSpeed)
                _rb.velocity = _rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        _exitingSlope = true;

        // reset y velocity
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        _exitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
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
}