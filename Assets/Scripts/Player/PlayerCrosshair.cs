using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerCrosshair : MonoBehaviour
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
    
    public event Action<Vector3, GameObject> OnSwingableTargetFound;
    public event Action OnSwingableTargetLost;
    
    public bool IsTargetingSwingable { get; private set; } = false;
    public Vector3 CurrentHitPoint { get; private set; }
    public GameObject CurrentSwingableTarget { get; private set; }
    
    private bool isSwinging = false;
    
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        SetupCrosshair();
    }
    
    void Update()
    {
        if (!isSwinging)
        {
            PerformCrosshairRaycast();
        }
        
        UpdateCrosshairColor();
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
    
    private void PerformCrosshairRaycast()
    {
        if (playerCamera == null) return;
        
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRopeDistance, swingableLayerMask))
        {
            if (!IsTargetingSwingable || CurrentSwingableTarget != hit.collider.gameObject)
            {
                IsTargetingSwingable = true;
                CurrentHitPoint = hit.point;
                CurrentSwingableTarget = hit.collider.gameObject;
                OnSwingableTargetFound?.Invoke(CurrentHitPoint, CurrentSwingableTarget);
            }
            else
            {
                CurrentHitPoint = hit.point;
            }
            
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
        }
        else
        {
            if (IsTargetingSwingable)
            {
                IsTargetingSwingable = false;
                CurrentSwingableTarget = null;
                OnSwingableTargetLost?.Invoke();
            }
            
            Debug.DrawRay(ray.origin, ray.direction * maxRopeDistance, Color.red);
        }
    }
    
    private void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        
        Color targetColor = defaultColor;
        if (isSwinging)
            targetColor = swingingColor;
        else if (IsTargetingSwingable)
            targetColor = highlightColor;
        
        crosshairImage.color = targetColor;
    }
    
    public void SetSwingingState(bool swinging)
    {
        isSwinging = swinging;
    }
} 