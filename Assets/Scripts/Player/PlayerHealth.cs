using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private HealthBarFill healthBarFill;
    [SerializeField] private float maxHealth = 100;

    private float _health;
    private void Start()
    {
        _health = maxHealth;
        healthBarFill.SetMaxHealth(maxHealth);
    }

    private void Update()
    {

    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            
            Debug.Log("You are dead");
        }
        healthBarFill.SetHealth(_health);
    }
}
