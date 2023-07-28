using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DebugCameraController : MonoBehaviour
{
    public float MaximumFOV = 100f;
    public float MinimumFOV = 10f;
    public float ZoomSpeed = 5f;
    public float Speed = 10f;
    public float Sensitivity = 5f;
    bool CursorLocked
    {
        get
        {
            return Cursor.lockState == CursorLockMode.Locked;
        }
        set
        {
            if (value)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    float FOV
    {
        get
        {
            return _camera.fieldOfView;
        }
        set
        {
            _camera.fieldOfView = value;
        }
    }

    float _rotationX = 0f;
    float _rotationY = 0f;
    Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        CursorLocked = true;
    }
    void Update()
    {
        if (CursorLocked)
        {
            var currentSpeed = Speed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);
            var forwardAxis = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            var rightAxis = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);

            var mouseX = Input.GetAxisRaw("Mouse X");
            var mouseY = Input.GetAxisRaw("Mouse Y");

            _rotationY += mouseX * Sensitivity;
            _rotationX -= mouseY * Sensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
            transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
            transform.position += transform.forward * forwardAxis * currentSpeed * Time.deltaTime;
            transform.position += transform.right * rightAxis * currentSpeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
            CursorLocked = !CursorLocked;
        var fov = FOV;
        fov -= (Input.GetKey(KeyCode.Z) ? 1f : 0f) * ZoomSpeed * Time.deltaTime;
        fov += (Input.GetKey(KeyCode.X) ? 1f : 0f) * ZoomSpeed * Time.deltaTime;
        FOV = Mathf.Clamp(fov, MinimumFOV, MaximumFOV);
    }
}