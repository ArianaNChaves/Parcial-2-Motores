using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    
    private Rigidbody _rigidbody;
    private Vector3 _inputMovement;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        InputDetection();
    }
    
    private void FixedUpdate()
    {
        Movement();
    }

    private void InputDetection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        _inputMovement = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void Movement()
    {
        _rigidbody.velocity = new Vector3(_inputMovement.x, 0f, _inputMovement.z) * speed;
    }
}
