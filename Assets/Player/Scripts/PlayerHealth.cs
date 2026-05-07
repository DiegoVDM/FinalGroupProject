using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damagePerHit = 25f;
    [SerializeField] private float healthRegenerationPerSecond = 5f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;

    [SerializeField] private float currentHealth;

    private float invincibleUntilTime;
    private bool hasLoggedDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float DamagePerHit => damagePerHit;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsInvincible => Time.time < invincibleUntilTime;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (IsDead || currentHealth >= maxHealth)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth + healthRegenerationPerSecond * Time.deltaTime,
            0f,
            maxHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f || IsDead || IsInvincible)
            return;

        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, maxHealth);

        if (IsDead)
        {
            if (!hasLoggedDeath)
            {
                Debug.Log("Player died");
                hasLoggedDeath = true;
            }

            return;
        }

        invincibleUntilTime = Time.time + invincibilityDuration;
    }

    void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        damagePerHit = Mathf.Max(0f, damagePerHit);
        healthRegenerationPerSecond = Mathf.Max(0f, healthRegenerationPerSecond);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
}
