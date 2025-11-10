using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;
    public bool isInvincible;
    
    [Tooltip("Drag and drop the health bar from canvas onto here.")]
    [SerializeField] private HealthBar healthBar;

    public float RemainingHealthPercentage
    {
        get { return currentHealth / maxHealth; }
    }

    [Tooltip("Drag player to the box below and assign PlayerInvincibilityDamaged.StartInvincibility.")]
    public UnityEvent OnDamaged;
    [Tooltip("Drag player to the box below and assign PlayerController.enabled.")]
    public UnityEvent OnDeath;
    
    public UnityEvent OnHealed;

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        
        if(isInvincible) 
        {
            return;
        }
        
        currentHealth -= damage;
        
        if (healthBar == null)
        {
            Debug.LogError("HealthBar is not assigned on " + gameObject.name);
            return;
        }
        
        healthBar.SetValue((int)currentHealth);

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (currentHealth == 0)
        {
            OnDeath.Invoke();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        else
        {
            OnDamaged.Invoke();
        }
    }

    public void AddHealth(float amount)
    {
        if (currentHealth == maxHealth)
        {
            OnHealed?.Invoke();
            return;
        }
        
        currentHealth += amount;
        Debug.Log(currentHealth);
        Debug.Log(amount);
        healthBar.SetValue((int)currentHealth);
        OnHealed?.Invoke();
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
            healthBar.SetValue((int)currentHealth);
        }
        

    }
    
}