using Pathfinding;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    protected int startingHealth = 100;
    [SerializeField] protected int currentHealth;
    //public AudioClip deathSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    //private Flash flash;
    public UnityEvent onDeath;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashDuration = 0.2f;
    public int StartingHealth
    {
        get => startingHealth;
        set => startingHealth = value;
    }
    private bool isDead = false;

    // TODO - Fix health starting at 0 when loading due to currentHealth starting at 0
    // TODO - Fix position starting at (0,0,0) when loading due to not tracking position when starting new game
    public void LoadData(GameData data)
    {
        data.enemiesKilled.TryGetValue(id, out isDead);
        //data.enemiesHealth.TryGetValue(id, out currentHealth);
        //data.enemiesPosition.TryGetValue(id, out Vector3 position);

        //transform.position = position;

        if (isDead)
        {
            gameObject.SetActive(false);
        }
    }

    // TODO - Fix position not being saved properly because enemy is destroyed on death
    public void SaveData(ref GameData data)
    {
        if (data.enemiesKilled.ContainsKey(id))
        {
            data.enemiesKilled.Remove(id);
        }

        //if (data.enemiesHealth.ContainsKey(id))
        //{
        //    data.enemiesHealth.Remove(id);
        //}

        //if (data.enemiesPosition.ContainsKey(id))
        //{
        //    data.enemiesPosition.Remove(id);
        //}

        data.enemiesKilled.Add(id, isDead);
        //data.enemiesHealth.Add(id, currentHealth);
        //data.enemiesPosition.Add(id, transform.position);
    }

    private void Start()
    {
        currentHealth = StartingHealth;
        audioSource = GetComponent<AudioSource>();

        
        rb = GetComponent<Rigidbody2D>();
        //flash = GetComponent<Flash>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Damage the enemy
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            if (audioSource != null && hurtSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
                audioSource.PlayOneShot(hurtSound);
            }

            if (spriteRenderer != null)
                StartCoroutine(FlashWhite());
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            DetectDeath();
        }
        Debug.Log(currentHealth);
        //StartCoroutine(flash.FlashRoutine());
    }

    // Check if the enemy is dead
    public virtual void DetectDeath()
    {
        onDeath.Invoke();
        if (audioSource != null && deathSound != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            audioSource.PlayOneShot(deathSound, 0.3f);
        }
    }

    public void GetCurrentHealth()
    {

    }

    public bool IsDead()
    { 
        return currentHealth <= 0;
    }
    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

}
