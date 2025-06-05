using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class GrapplingSwingSystem : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite crosshairSprite;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private Color swingingColor = Color.yellow;
    
    [Header("Raycast Settings")]
    [SerializeField] private float maxRopeDistance = 50f;
    [SerializeField] private LayerMask swingableLayerMask = 1;
    [SerializeField] private Camera playerCamera;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode shortenRopeKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode extendRopeKey = KeyCode.Space;
    
    [Header("Rope Visual Settings")]
    [SerializeField] private LineRenderer ropeLineRenderer;
    [SerializeField] private Transform ropeOrigin;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private float ropeWidth = 0.1f;
    [SerializeField] private int ropeSegments = 20;
    
    [Header("Swing Physics Settings")]
    [SerializeField] private float swingForce = 20f;
    [SerializeField] private float ropeAdjustSpeed = 5f;
    [SerializeField] private float minRopeLength = 3f;
    [SerializeField] private float maxRopeLength = 50f;
    [SerializeField] private float springForce = 100f;
    [SerializeField] private float dampingForce = 10f;
    [SerializeField] private float airControl = 2f;
    [SerializeField] private float momentumPreservation = 1.2f;
    [SerializeField] private float releaseBoostForce = 5f;
    
    private bool isTargetingSwingable = false;
    private Vector3 currentHitPoint;
    private GameObject currentSwingableTarget;
    
    private bool isSwinging = false;
    private Vector3 ropeAttachPoint;
    private float currentRopeLength;
    private float targetRopeLength;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (ropeOrigin == null)
        {
            ropeOrigin = transform;
        }
        
        SetupCrosshair();
        SetupRopeLineRenderer();
    }
    
    void Update()
    {
        if (!isSwinging)
        {
            PerformCrosshairRaycast();
            UpdateCrosshairColor();
            HandleRopeInput();
        }
        else
        {
            HandleRopeLengthAdjustment();
            
            if (crosshairImage != null)
                crosshairImage.color = swingingColor;
            
            if (Input.GetKeyUp(swingKey) || playerMovement.IsGrounded())
            {
                ReleaseRope();
            }
        }
        
        UpdateRopeVisual();
    }
    
    void FixedUpdate()
    {
        if (isSwinging)
        {
            ApplySwingPhysics();
            ApplyAirControl();
        }
    }
    
    private void SetupCrosshair()
    {
        if (crosshairImage != null && crosshairSprite != null)
        {
            crosshairImage.sprite = crosshairSprite;
            crosshairImage.color = defaultColor;
            
            RectTransform crosshairRect = crosshairImage.GetComponent<RectTransform>();
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;
        }
    }
    
    private void SetupRopeLineRenderer()
    {
        if (ropeLineRenderer == null)
        {
            ropeLineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        ropeLineRenderer.material = ropeMaterial;
        ropeLineRenderer.startWidth = ropeWidth;
        ropeLineRenderer.endWidth = ropeWidth;
        ropeLineRenderer.positionCount = ropeSegments;
        ropeLineRenderer.useWorldSpace = true;
        ropeLineRenderer.enabled = false;
        
        ropeLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ropeLineRenderer.receiveShadows = false;
    }
    
    private void PerformCrosshairRaycast()
    {
        if (playerCamera == null) return;
        
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRopeDistance, swingableLayerMask))
        {
            isTargetingSwingable = true;
            currentHitPoint = hit.point;
            currentSwingableTarget = hit.collider.gameObject;
            
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
        }
        else
        {
            isTargetingSwingable = false;
            currentSwingableTarget = null;
            
            Debug.DrawRay(ray.origin, ray.direction * maxRopeDistance, Color.red);
        }
    }
    
    private void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        
        Color targetColor = defaultColor;
        if (isSwinging)
            targetColor = swingingColor;
        else if (isTargetingSwingable)
            targetColor = highlightColor;
        
        crosshairImage.color = targetColor;
    }
    
    private void HandleRopeInput()
    {
        if (Input.GetKeyDown(swingKey))
        {
            if (isTargetingSwingable && currentSwingableTarget != null)
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
        
        if (adjustment != 0f)
        {
            targetRopeLength = Mathf.Clamp(targetRopeLength + adjustment, minRopeLength, maxRopeLength);
        }
    }
    
    private void StartSwinging()
    {
        isSwinging = true;
        ropeAttachPoint = currentHitPoint;
        
        currentRopeLength = Vector3.Distance(ropeOrigin.position, ropeAttachPoint);
        targetRopeLength = currentRopeLength;
        
        targetRopeLength = Mathf.Clamp(targetRopeLength, minRopeLength, maxRopeLength);
        currentRopeLength = targetRopeLength;
        
        ropeLineRenderer.enabled = true;
        
        Debug.Log($"Started swinging to: {currentSwingableTarget.name} at distance: {currentRopeLength:F2}");
    }
    
    private void ReleaseRope()
    {
        if (!isSwinging) return;
        
        Vector3 currentVelocity = playerMovement.GetVelocity();
        
        Vector3 swingDirection = (ropeOrigin.position - ropeAttachPoint).normalized;
        
        Vector3 ropeDirection = (ropeAttachPoint - ropeOrigin.position).normalized;
        Vector3 swingVelocity = Vector3.ProjectOnPlane(currentVelocity, ropeDirection);
        
        Vector3 preservedVelocity = currentVelocity * momentumPreservation;
        
        if (swingVelocity.magnitude > 0.5f)
        {
            Vector3 swingBoost = swingVelocity.normalized * releaseBoostForce;
            preservedVelocity += swingBoost;
        }
        
        playerMovement.SetVelocity(preservedVelocity);
        
        isSwinging = false;
        ropeLineRenderer.enabled = false;
        
        Debug.Log($"Released rope with velocity: {preservedVelocity.magnitude:F2} (swing velocity: {swingVelocity.magnitude:F2})");
    }
    
    private void ApplySwingPhysics()
    {
        if (!isSwinging || ropeOrigin == null) return;
        
        Vector3 ropeVector = ropeOrigin.position - ropeAttachPoint;
        float currentDistance = ropeVector.magnitude;
        
        currentRopeLength = Mathf.Lerp(currentRopeLength, targetRopeLength, Time.fixedDeltaTime * 2f);
        
        if (currentDistance > currentRopeLength)
        {
            Vector3 ropeDirection = ropeVector.normalized;
            Vector3 targetPosition = ropeAttachPoint + ropeDirection * currentRopeLength;
            Vector3 correctionVector = targetPosition - ropeOrigin.position;
            
            Vector3 springForceVector = correctionVector * springForce;
            playerMovement.AddExternalForce(springForceVector, ForceMode.Force);
            
            Vector3 velocityTowardsAttach = Vector3.Project(playerMovement.GetVelocity(), -ropeDirection);
            Vector3 dampingForceVector = -velocityTowardsAttach * dampingForce;
            playerMovement.AddExternalForce(dampingForceVector, ForceMode.Force);
        }
        
        playerMovement.AddExternalForce(Vector3.up * (Physics.gravity.magnitude * 0.1f), ForceMode.Acceleration);
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
    
    private void UpdateRopeVisual()
    {
        if (!ropeLineRenderer.enabled || !isSwinging) return;
        
        Vector3 startPoint = ropeOrigin.position;
        Vector3 endPoint = ropeAttachPoint;
        
        for (int i = 0; i < ropeSegments; i++)
        {
            float t = (float)i / (ropeSegments - 1);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);
            
            float sag = Mathf.Sin(t * Mathf.PI) * (currentRopeLength * 0.1f);
            point.y -= sag;
            
            ropeLineRenderer.SetPosition(i, point);
        }
    }
    
    public bool IsSwinging()
    {
        return isSwinging;
    }
    
    public bool IsTargetingSwingable()
    {
        return isTargetingSwingable;
    }
    
    public float GetCurrentRopeLength()
    {
        return currentRopeLength;
    }
    
    public float GetTargetRopeLength()
    {
        return targetRopeLength;
    }
    
    public void ForceReleaseRope()
    {
        ReleaseRope();
    }
} 