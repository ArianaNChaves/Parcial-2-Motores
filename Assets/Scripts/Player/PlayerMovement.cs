using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField, Range(0,10f)] private float rayCastRadius = 10f;
    [SerializeField, Range(0, 10f)] private float jumpRayCast = 10f;
    [SerializeField] private LayerMask groundMask;
    
    private Rigidbody _rigidbody;
    private Vector3 _direction;
    private PlayerRotation _playerRotation;
    private bool _isGrounding = true;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerRotation = GetComponent<PlayerRotation>();
    }

    private void Update()
    {
        InputDetection();
        JumpHandler();
    }
    
    private void FixedUpdate()
    {
        Movement();
    }

    private void InputDetection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        _direction = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void Movement()
    {
        float vy = _rigidbody.velocity.y;
        
        Vector3 forward = Vector3.ProjectOnPlane(_playerRotation.GetTransform().forward, Vector3.up).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(_playerRotation.GetTransform().right,   Vector3.up).normalized;

        Vector3 moveDir = forward * _direction.z + right * _direction.x;
        moveDir.Normalize();

        Vector3 horizontalVel = moveDir * speed;
        _rigidbody.velocity   = new Vector3(horizontalVel.x, vy, horizontalVel.z);
        
    }

    private void JumpHandler()
    {
        _isGrounding = Physics.SphereCast(transform.position, rayCastRadius, Vector3.down, out RaycastHit hitInfo, jumpRayCast, groundMask);
        
        if (_isGrounding && Input.GetKeyDown(KeyCode.Space))
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 sphereCenter = transform.position + Vector3.down * jumpRayCast;
        Gizmos.DrawWireSphere(sphereCenter, rayCastRadius);
    }
}
