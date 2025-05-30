using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topClamp = 80f;  
    [SerializeField] private float bottomClamp = 80f;
    
    [Header("Camera ")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private float _xRotation = 0f; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _xRotation = virtualCamera.transform.eulerAngles.x;

    }

    private void Update()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -topClamp, bottomClamp); 
        
        virtualCamera.transform.Rotate(Vector3.up * mouseX);
        
        virtualCamera.transform.rotation = Quaternion.Euler(_xRotation, virtualCamera.transform.eulerAngles.y, 0f); 
    }

    public Transform GetTransform()
    {
        return virtualCamera.transform;
    }
}


