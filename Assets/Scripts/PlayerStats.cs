using UnityEngine;
using System;

//temp, will change after player is implemented.  not real file
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 6f;
    public float baseDamage   = 1f;

    [HideInInspector] public float healthMultiplier = 1f;
    [HideInInspector] public float speedMultiplier  = 1f;
    [HideInInspector] public float damageMultiplier = 1f;

    public float MaxHealth  => baseMaxHealth * healthMultiplier;
    public float MoveSpeed  => baseMoveSpeed  * speedMultiplier;
    public float Damage     => baseDamage     * damageMultiplier;

    public PerkType? ActivePerk { get; private set; } = null;
    public event Action<PerkType?> ActivePerkChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyPerk(PerkType perk)
    {
        // If your design becomes "multiple perks stack", remove these resets and
        // track active perks as a list instead. For now we treat the latest perk
        // as the single active perk displayed in UI.
        healthMultiplier = 1f;
        speedMultiplier = 1f;
        damageMultiplier = 1f;

        switch (perk)
        {
            case PerkType.DoubleHealth:    healthMultiplier = 2f;  break;
            case PerkType.Speed:           speedMultiplier  = 1.5f; break;
            case PerkType.StrongerDamage:  damageMultiplier = 2f;  break;
        }

        ActivePerk = perk;
        ActivePerkChanged?.Invoke(ActivePerk);
    }
}

public enum PerkType { DoubleHealth, Speed, StrongerDamage }