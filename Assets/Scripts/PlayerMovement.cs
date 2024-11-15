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
    
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;

    private Vector2 MoveVector { get; set; }
    private bool JumpButton { get; set; }
    
    private Rigidbody _rb;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        GetInput();
        Move();
    }
    
    private void GetInput()
    {
        MoveVector = moveAction.action.ReadValue<Vector2>();
        JumpButton = jumpAction.action.triggered;
    }

    private void Move()
    {
        _rb.AddForce(new Vector3(MoveVector.x * speed, 0, MoveVector.y * speed));
        
        if (JumpButton) _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
