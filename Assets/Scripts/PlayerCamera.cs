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
        _lookVector = lookAction.action.ReadValue<Vector2>() * (Time.deltaTime * sensitivity);
        
        _yRotation += _lookVector.x;
        
        _xRotation -= _lookVector.y;
        
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
        
        transform.localPosition = cameraPosition.position;
    }
    
    public void DoFov(float fovTarget)
    {
        LMotion.Create(_camera.fieldOfView, fovTarget, 0.25f).WithEase(Ease.InSine).Bind(x => _camera.fieldOfView = x);
    }
    
    public void DoTilt(float tiltTarget)
    {
        LMotion.Create(transform.localRotation.z, tiltTarget, 0.25f).WithEase(Ease.OutQuint).Bind(x => transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, x, transform.localRotation.w));
    }
}
