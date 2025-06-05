using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    [Header("Rope Visual Settings")]
    [SerializeField] private LineRenderer ropeLineRenderer;
    [SerializeField] private Transform ropeOrigin;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private float ropeWidth = 0.1f;
    [SerializeField] private int ropeSegments = 20;
    
    [Header("Rope Physics Settings")]
    [SerializeField] private float ropeAdjustSpeed = 5f;
    [SerializeField] private float minRopeLength = 3f;
    [SerializeField] private float maxRopeLength = 50f;
    [SerializeField] private float springForce = 100f;
    [SerializeField] private float dampingForce = 10f;
    
    public bool IsRopeActive { get; private set; } = false;
    public Vector3 RopeAttachPoint { get; private set; }
    public float CurrentRopeLength { get; private set; }
    public float TargetRopeLength { get; private set; }
    public Transform RopeOrigin => ropeOrigin;
    
    void Start()
    {
        if (ropeOrigin == null)
        {
            ropeOrigin = transform;
        }
        
        SetupRopeLineRenderer();
    }
    
    void Update()
    {
        if (IsRopeActive)
        {
            UpdateRopeVisual();
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
    
    public void AttachRope(Vector3 attachPoint)
    {
        IsRopeActive = true;
        RopeAttachPoint = attachPoint;
        
        CurrentRopeLength = Vector3.Distance(ropeOrigin.position, RopeAttachPoint);
        TargetRopeLength = CurrentRopeLength;
        
        TargetRopeLength = Mathf.Clamp(TargetRopeLength, minRopeLength, maxRopeLength);
        CurrentRopeLength = TargetRopeLength;
        
        ropeLineRenderer.enabled = true;
        
        Debug.Log($"Rope attached at distance: {CurrentRopeLength:F2}");
    }
    
    public void DetachRope()
    {
        if (!IsRopeActive) return;
        
        IsRopeActive = false;
        ropeLineRenderer.enabled = false;
        
        Debug.Log("Rope detached");
    }
    
    public void AdjustRopeLength(float adjustment)
    {
        if (!IsRopeActive) return;
        
        if (adjustment != 0f)
        {
            TargetRopeLength = Mathf.Clamp(TargetRopeLength + adjustment, minRopeLength, maxRopeLength);
        }
    }
    
    public void UpdateRopeLength()
    {
        if (!IsRopeActive) return;
        
        CurrentRopeLength = Mathf.Lerp(CurrentRopeLength, TargetRopeLength, Time.fixedDeltaTime * 2f);
    }
    
    public Vector3 CalculateRopeConstraintForce(PlayerMovement playerMovement)
    {
        if (!IsRopeActive || ropeOrigin == null) return Vector3.zero;
        
        Vector3 ropeVector = ropeOrigin.position - RopeAttachPoint;
        float currentDistance = ropeVector.magnitude;
        
        Vector3 totalForce = Vector3.zero;
        
        if (currentDistance > CurrentRopeLength)
        {
            Vector3 ropeDirection = ropeVector.normalized;
            Vector3 targetPosition = RopeAttachPoint + ropeDirection * CurrentRopeLength;
            Vector3 correctionVector = targetPosition - ropeOrigin.position;
            
            Vector3 springForceVector = correctionVector * springForce;
            totalForce += springForceVector;
            
            Vector3 velocityTowardsAttach = Vector3.Project(playerMovement.GetVelocity(), -ropeDirection);
            Vector3 dampingForceVector = -velocityTowardsAttach * dampingForce;
            totalForce += dampingForceVector;
        }
        
        totalForce += Vector3.up * (Physics.gravity.magnitude * 0.1f);
        
        return totalForce;
    }
    
    private void UpdateRopeVisual()
    {
        if (!ropeLineRenderer.enabled || !IsRopeActive) return;
        
        Vector3 startPoint = ropeOrigin.position;
        Vector3 endPoint = RopeAttachPoint;
        
        for (int i = 0; i < ropeSegments; i++)
        {
            float t = (float)i / (ropeSegments - 1);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);
            
            float sag = Mathf.Sin(t * Mathf.PI) * (CurrentRopeLength * 0.1f);
            point.y -= sag;
            
            ropeLineRenderer.SetPosition(i, point);
        }
    }
} 