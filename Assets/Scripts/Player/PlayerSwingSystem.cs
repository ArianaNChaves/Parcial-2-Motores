using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerCrosshair))]
[RequireComponent(typeof(PlayerRope))]
public class PlayerSwingSystem : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode shortenRopeKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode extendRopeKey = KeyCode.Space;
    
    [Header("Swing Physics Settings")]
    [SerializeField] private float swingForce = 20f;
    [SerializeField] private float ropeAdjustSpeed = 5f;
    [SerializeField] private float airControl = 2f;
    [SerializeField] private float momentumPreservation = 1.2f;
    [SerializeField] private float releaseBoostForce = 5f;
    [SerializeField] private Camera playerCamera;
    
    private bool isSwinging = false;
    private PlayerMovement playerMovement;
    private PlayerCrosshair crosshair;
    private PlayerRope rope;
    
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        crosshair = GetComponent<PlayerCrosshair>();
        rope = GetComponent<PlayerRope>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Subscribe to crosshair events
        crosshair.OnSwingableTargetFound += OnSwingableTargetFound;
        crosshair.OnSwingableTargetLost += OnSwingableTargetLost;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (crosshair != null)
        {
            crosshair.OnSwingableTargetFound -= OnSwingableTargetFound;
            crosshair.OnSwingableTargetLost -= OnSwingableTargetLost;
        }
    }
    
    void Update()
    {
        if (!isSwinging)
        {
            HandleRopeInput();
        }
        else
        {
            HandleRopeLengthAdjustment();
            
            if (Input.GetKeyUp(swingKey) || playerMovement.IsGrounded())
            {
                ReleaseRope();
            }
        }
    }
    
    void FixedUpdate()
    {
        if (isSwinging)
        {
            rope.UpdateRopeLength();
            ApplySwingPhysics();
            ApplyAirControl();
        }
    }
    
    private void OnSwingableTargetFound(Vector3 hitPoint, GameObject target)
    {
        // Target found - crosshair will handle visual feedback
    }
    
    private void OnSwingableTargetLost()
    {
        // Target lost - crosshair will handle visual feedback
    }
    
    private void HandleRopeInput()
    {
        if (Input.GetKeyDown(swingKey))
        {
            if (crosshair.IsTargetingSwingable && crosshair.CurrentSwingableTarget != null)
            {
                StartSwinging();
            }
        }
    }
    
    private void HandleRopeLengthAdjustment()
    {
        float adjustment = 0f;
        
        if (Input.GetKey(shortenRopeKey))
        {
            adjustment = -ropeAdjustSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(extendRopeKey))
        {
            adjustment = ropeAdjustSpeed * Time.deltaTime;
        }
        
        rope.AdjustRopeLength(adjustment);
    }
    
    private void StartSwinging()
    {
        isSwinging = true;
        crosshair.SetSwingingState(true);
        rope.AttachRope(crosshair.CurrentHitPoint);
        
        Debug.Log($"Started swinging to: {crosshair.CurrentSwingableTarget.name}");
    }
    
    private void ReleaseRope()
    {
        if (!isSwinging) return;
        
        Vector3 currentVelocity = playerMovement.GetVelocity();
        
        // Calculate swing direction and velocity
        Vector3 ropeDirection = (rope.RopeAttachPoint - rope.RopeOrigin.position).normalized;
        Vector3 swingVelocity = Vector3.ProjectOnPlane(currentVelocity, ropeDirection);
        
        // Preserve momentum with boost
        Vector3 preservedVelocity = currentVelocity * momentumPreservation;
        
        if (swingVelocity.magnitude > 0.5f)
        {
            Vector3 swingBoost = swingVelocity.normalized * releaseBoostForce;
            preservedVelocity += swingBoost;
        }
        
        playerMovement.SetVelocity(preservedVelocity);
        
        isSwinging = false;
        crosshair.SetSwingingState(false);
        rope.DetachRope();
        
        Debug.Log($"Released rope with velocity: {preservedVelocity.magnitude:F2} (swing velocity: {swingVelocity.magnitude:F2})");
    }
    
    private void ApplySwingPhysics()
    {
        if (!isSwinging) return;
        
        Vector3 ropeForce = rope.CalculateRopeConstraintForce(playerMovement);
        playerMovement.AddExternalForce(ropeForce, ForceMode.Force);
    }
    
    private void ApplyAirControl()
    {
        if (!isSwinging) return;
        
        Vector3 movementDirection = playerMovement.GetMovementDirection();
        
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 moveDirection = (cameraForward * movementDirection.z + cameraRight * movementDirection.x).normalized;
        
        if (moveDirection.magnitude > 0.1f)
        {
            playerMovement.AddExternalForce(moveDirection * airControl, ForceMode.Force);
        }
    }
    
    // Public methods for external access
    public bool IsSwinging()
    {
        return isSwinging;
    }
    
    public bool IsTargetingSwingable()
    {
        return crosshair.IsTargetingSwingable;
    }
    
    public float GetCurrentRopeLength()
    {
        return rope.CurrentRopeLength;
    }
    
    public float GetTargetRopeLength()
    {
        return rope.TargetRopeLength;
    }
    
    public void ForceReleaseRope()
    {
        ReleaseRope();
    }
} 