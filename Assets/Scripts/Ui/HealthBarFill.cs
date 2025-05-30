using UnityEngine;
using UnityEngine.UI;

public class HealthBarFill : MonoBehaviour
{
    [SerializeField] private Image foregroundImage;
    
    private float _maxHealth;
    private float _currentHealth;

    private void Start()
    {
        _currentHealth = _maxHealth;
        UpdateBar();
    }
    
    public void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0f, _maxHealth);
        UpdateBar();
    }

    public void SetMaxHealth(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = _maxHealth;
        UpdateBar();   
    }

    private void UpdateBar()
    {
        foregroundImage.fillAmount = _currentHealth / _maxHealth;
    }
}

