using UnityEngine;
using UnityEngine.InputSystem;
using LitMotion;
using UnityEngine.Rendering.VirtualTexturing;

public class PlayerCamera : MonoBehaviour
{
    
    [SerializeField] private InputActionReference lookAction;
    private Camera _camera;
    
    public Transform cameraPosition;
    public Transform cameraHolder;
    public Transform orientation;
    public int baseFov;
    public float sensitivity;
    
    private Vector2 _lookVector;
    
    private float _xRotation;
    private float _yRotation;
    private float _zRotation;

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
        _camera = GetComponent<Camera>();
        _camera.fieldOfView = baseFov;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (MenuManager.Instance.isPaused) return;
        
        _lookVector = lookAction.action.ReadValue<Vector2>() * (Time.deltaTime * sensitivity);
        
        _yRotation += _lookVector.x;
        
        _xRotation -= _lookVector.y;
        
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, _zRotation);
        orientation.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
        
        cameraHolder.position = cameraPosition.position;
        transform.position = cameraPosition.position;
    }
    
    public void DoSprintSlideFov(float fovTarget)
    {
        LMotion.Create(_camera.fieldOfView, fovTarget, 0.1f).WithEase(Ease.InSine).Bind(x => _camera.fieldOfView = x);
    }
    
    public void DoWallRunFov(float fovTarget)
    {
        LMotion.Create(_camera.fieldOfView, fovTarget, 0.25f).WithEase(Ease.InSine).Bind(x => _camera.fieldOfView = x);
    }
    
    public void DoWallRunTilt(float tiltTarget)
    {
        LMotion.Create(_zRotation, tiltTarget, 0.25f).WithEase(Ease.InSine).Bind(x => _zRotation = x);
    }
    
    public void SetSensitivity(string newSensitivity)
    {
        sensitivity = float.Parse(newSensitivity);
    }
}
