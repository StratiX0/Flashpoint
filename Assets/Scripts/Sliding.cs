using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody _rb;
    private PlayerMovement _playerMovement;
    
    [Header("Sliding Settings")]
    public float slideForce;

    public float slideYScale;
    public float standYScale;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference crouchAction;
    private Vector2 MoveVector { get; set; }
    private float CrouchButton { get; set; }
    
    private void Awake()
    {
        moveAction.action.Enable();
        crouchAction.action.Enable();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        crouchAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        crouchAction.action.Disable();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = playerObj.GetComponent<PlayerMovement>();
        
        standYScale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        
        if (CrouchButton > 0f && MoveVector != Vector2.zero)
        {
            StartSlide();
        }
        else if (CrouchButton == 0f && _playerMovement.sliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (_playerMovement.sliding)
        {
            SlidingMovement();
        }
    }

    private void GetInput()
    {
        MoveVector = moveAction.action.ReadValue<Vector2>();
        CrouchButton = crouchAction.action.ReadValue<float>();
    }
    
    private void StartSlide()
    {
        _playerMovement.sliding = true;
        
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
    }
    
    private void SlidingMovement()
    {
        Vector3 moveDirection = orientation.forward * MoveVector.y + orientation.right * MoveVector.x;
        
        if (!_playerMovement.OnSlope() || _rb.velocity.y > 0.1f)
        {
            _rb.AddForce(moveDirection.normalized * slideForce, ForceMode.Force);
        }
        else
        {
            _rb.AddForce(_playerMovement.GetSlopeMoveDirection(moveDirection) * slideForce, ForceMode.Force);
        }
        
        if (_playerMovement.MovementSpeed <= _playerMovement.MinimumSlideSpeed)
        {
            StopSlide();
        }
    }
    
    private void StopSlide()
    {
        _playerMovement.sliding = false;
        
        playerObj.localScale = new Vector3(playerObj.localScale.x, standYScale, playerObj.localScale.z);
    }
}
