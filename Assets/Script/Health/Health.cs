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
    public bool isDashing;
    public float dashInvincibleTime = 0.2f;
    
    [Tooltip("Drag and drop the health bar from canvas onto here.")]
    [SerializeField] private HealthBar healthBar;

    public GameManager gameManager;
    private bool isDead;
    public float RemainingHealthPercentage
    {
        get { return currentHealth / maxHealth; }
    }

    [Tooltip("Drag player to the box below and assign PlayerInvincibilityDamaged.StartInvincibility.")]
    public UnityEvent OnDamaged;
    [Tooltip("Drag player to the box below and assign PlayerController.enabled.")]
    public UnityEvent OnDeath;
    
    public UnityEvent OnHealed;

    private void Awake()
    {
        TryAssignHealthBar();
    }

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

        if (isDashing)
        {
            StartCoroutine(EndInvincibleRoutine());
            return;
        }
        
        currentHealth -= damage;
        
        if (healthBar == null)
        {
            TryAssignHealthBar();
            if (healthBar == null)
            {
                Debug.LogError("HealthBar is not assigned or found for " + gameObject.name);
                return;
            }
        }
        
        healthBar.SetValue((int)currentHealth);

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (currentHealth == 0 && !isDead)
        {
            isDead = true;
            OnDeath.Invoke();
            gameManager.GameOver();
        }
        else
        {
            OnDamaged.Invoke();
        }
    }

    private IEnumerator EndInvincibleRoutine()
    {
        yield return new WaitForSeconds(dashInvincibleTime);
        isDashing = false;
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
        if (healthBar == null)
        {
            TryAssignHealthBar();
            if (healthBar == null)
            {
                Debug.LogError("HealthBar is not assigned or found for " + gameObject.name);
                return;
            }
        }
        healthBar.SetValue((int)currentHealth);
        OnHealed?.Invoke();
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
            healthBar.SetValue((int)currentHealth);
        }
        

    }
    
    private void TryAssignHealthBar()
    {
        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
            if (healthBar == null)
            {
                Debug.LogWarning("Health: No HealthBar found in the scene for " + gameObject.name);
            }
        }
    }
    
}
