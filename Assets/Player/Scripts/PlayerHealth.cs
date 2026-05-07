using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damagePerHit = 25f;
    [SerializeField] private float healthRegenerationPerSecond = 5f;

    [Header("Perks")]
    public PerkType? ActivePerk { get; private set; } = null;
    public event Action<PerkType?> ActivePerkChanged;

    [SerializeField] private float healthMultiplier = 1f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float speedMultiplier = 1f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;

    [SerializeField] private float currentHealth;

    private float invincibleUntilTime;
    private bool hasLoggedDeath;

    public float MaxHealth => maxHealth * healthMultiplier;
    public float CurrentHealth => currentHealth;
    public float DamagePerHit => damagePerHit * damageMultiplier;
    public float SpeedMultiplier => speedMultiplier;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsInvincible => Time.time < invincibleUntilTime;

    void Awake()
    {
        Instance = this;

        SyncFromPerkState();
        currentHealth = Mathf.Clamp(currentHealth <= 0f ? MaxHealth : currentHealth, 0f, MaxHealth);
    }

    void Update()
    {
        float mh = MaxHealth;
        if (IsDead || currentHealth >= mh)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth + healthRegenerationPerSecond * Time.deltaTime,
            0f,
            mh);
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f || IsDead || IsInvincible)
            return;

        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, MaxHealth);

        if (IsDead)
        {
            if (!hasLoggedDeath)
            {
                Debug.Log("Player died");
                hasLoggedDeath = true;

                if (CurrencyManager.Instance != null)
                    CurrencyManager.Instance.CashOutOnDeath();
            }

            return;
        }

        invincibleUntilTime = Time.time + invincibilityDuration;
    }

    public void ApplyPerk(PerkType perk)
    {
        PerkState.Apply(perk);
        SyncFromPerkState();
        currentHealth = Mathf.Clamp(currentHealth, 0f, MaxHealth);
        ActivePerkChanged?.Invoke(ActivePerk);
    }

    void SyncFromPerkState()
    {
        PerkState.EnsureLoaded();

        ActivePerk = PerkState.ActivePerk;
        healthMultiplier = PerkState.HealthMultiplier;
        damageMultiplier = PerkState.DamageMultiplier;
        speedMultiplier = PerkState.SpeedMultiplier;
    }

    void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        damagePerHit = Mathf.Max(0f, damagePerHit);
        healthRegenerationPerSecond = Mathf.Max(0f, healthRegenerationPerSecond);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
        healthMultiplier = Mathf.Max(0.01f, healthMultiplier);
        damageMultiplier = Mathf.Max(0.01f, damageMultiplier);
        speedMultiplier = Mathf.Max(0.01f, speedMultiplier);
        currentHealth = Mathf.Clamp(currentHealth, 0f, MaxHealth);
    }
}
