using UnityEngine;
using UnityEngine.UI;

public class HealthBarFill : MonoBehaviour
{
    [SerializeField] private Image foregroundImage;

    [Header("Max health value")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }
    
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        UpdateBar();
    }

    private void UpdateBar()
    {
        foregroundImage.fillAmount = currentHealth / maxHealth;
    }
}

