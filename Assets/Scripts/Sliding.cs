using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    
    private Vector3 _moveInput;
    private float _crouchInput;
    private float _sprintInput;

    private void Awake()
    {
        moveAction.action.Enable();
        crouchAction.action.Enable();
        sprintAction.action.Enable();
    }
    
    private void OnEnable()
    {
        moveAction.action.Enable();
        crouchAction.action.Enable();
        sprintAction.action.Enable();
    }
    
    private void OnDisable()
    {
        moveAction.action.Disable();
        crouchAction.action.Disable();
        sprintAction.action.Disable();
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        _moveInput = moveAction.action.ReadValue<Vector2>();
        _crouchInput = crouchAction.action.ReadValue<float>();
        _sprintInput = sprintAction.action.ReadValue<float>();
        
        if (_crouchInput != 0f && pm.grounded && _moveInput != Vector3.zero)
            StartSlide();

        if (_crouchInput == 0f && pm.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;

        // sliding normal
        if(!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
