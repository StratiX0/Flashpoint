using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    
    [SerializeField] private InputActionReference lookAction;
    
    public Transform cameraPosition;
    public Transform orientation;
    public float sensitivity;
    
    private Vector2 _lookVector;
    
    private float _xRotation;
    private float _yRotation;

    private void Awake()
    {
        lookAction.action.Enable();
    }
    
    private void OnEnable()
    {
        lookAction.action.Enable();
    }
    
    private void OnDisable()
    {
        lookAction.action.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        _lookVector = lookAction.action.ReadValue<Vector2>() * (Time.deltaTime * sensitivity);
        
        _yRotation += _lookVector.x;
        
        _xRotation -= _lookVector.y;
        
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
        
        transform.localPosition = cameraPosition.position;
    }
}
