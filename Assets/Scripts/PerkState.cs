using UnityEngine;

public static class PerkState
{
    const string KeyActivePerk = "ActivePerk";

    static bool _loaded;

    public static PerkType? ActivePerk { get; private set; }
    public static float HealthMultiplier { get; private set; } = 1f;
    public static float DamageMultiplier { get; private set; } = 1f;
    public static float SpeedMultiplier { get; private set; } = 1f;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        int perkInt = PlayerPrefs.GetInt(KeyActivePerk, -1);
        if (perkInt < 0)
        {
            ClearRuntimeOnly();
            return;
        }

        ActivePerk = (PerkType)perkInt;
        RecomputeMultipliers();
    }

    public static void Apply(PerkType perk)
    {
        _loaded = true;
        ActivePerk = perk;
        RecomputeMultipliers();

        PlayerPrefs.SetInt(KeyActivePerk, (int)perk);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        _loaded = true;
        ActivePerk = null;
        ClearRuntimeOnly();

        PlayerPrefs.SetInt(KeyActivePerk, -1);
        PlayerPrefs.Save();
    }

    static void RecomputeMultipliers()
    {
        HealthMultiplier = 1f;
        DamageMultiplier = 1f;
        SpeedMultiplier = 1f;

        if (ActivePerk == null) return;

        switch (ActivePerk.Value)
        {
            case PerkType.DoubleHealth:
                HealthMultiplier = 2f;
                break;
            case PerkType.Speed:
                SpeedMultiplier = 1.5f;
                break;
            case PerkType.StrongerDamage:
                DamageMultiplier = 2f;
                break;
        }
    }

    static void ClearRuntimeOnly()
    {
        HealthMultiplier = 1f;
        DamageMultiplier = 1f;
        SpeedMultiplier = 1f;
    }
}
