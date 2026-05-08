using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] private int startingCurrency = 500;
    public UnityEvent<int> onCurrencyChanged;

    [Header("Run Earnings")]
    [SerializeField] private int moneyPerKill = 25;
    [SerializeField] private int pendingKills = 0;
    [SerializeField] private int pendingEarnings = 0;

    const int FallbackStartingCurrency = 500;
    public const string KeyCurrency = "CurrencyManager.Currency";
    const string KeyPendingKills = "CurrencyManager.PendingKills";
    const string KeyPendingEarnings = "CurrencyManager.PendingEarnings";

    int _currency;
    public int Currency => _currency;
    public int PendingKills => pendingKills;
    public int PendingEarnings => pendingEarnings;
    public int MoneyPerKill => moneyPerKill;

    /// <summary> Kills and money from the most recent extract/death cash-out (shown on death/summary UI). </summary>
    public int LastRunKills { get; private set; }
    public int LastRunMoneyEarned { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        onCurrencyChanged?.Invoke(_currency);
    }

    public bool TrySpend(int amount)
    {
        if (_currency < amount) return false;
        _currency -= amount;
        SaveCurrencyOnly();
        onCurrencyChanged?.Invoke(_currency);
        return true;
    }

    public void Add(int amount)
    {
        _currency += amount;
        SaveCurrencyOnly();
        onCurrencyChanged?.Invoke(_currency);
    }

    public void RegisterKill(int kills = 1)
    {
        if (kills <= 0) return;
        int earnings = kills * moneyPerKill;
        pendingKills += kills;
        pendingEarnings += earnings;
        Add(earnings);
        SavePendingOnly();
    }

    public static int RegisterKillReward(int kills, int moneyPerKill, int defaultCurrency = FallbackStartingCurrency)
    {
        if (kills <= 0) return GetSavedCurrency(defaultCurrency);

        if (Instance != null)
        {
            Instance.RegisterKill(kills);
            return Instance.Currency;
        }

        int earnings = kills * Mathf.Max(1, moneyPerKill);
        int currency = GetSavedCurrency(defaultCurrency) + earnings;
        int pendingKillCount = PlayerPrefs.GetInt(KeyPendingKills, 0) + kills;
        int pendingEarned = PlayerPrefs.GetInt(KeyPendingEarnings, 0) + earnings;

        PlayerPrefs.SetInt(KeyCurrency, currency);
        PlayerPrefs.SetInt(KeyPendingKills, pendingKillCount);
        PlayerPrefs.SetInt(KeyPendingEarnings, pendingEarned);
        PlayerPrefs.Save();

        return currency;
    }

    public static int GetSavedCurrency(int defaultValue = FallbackStartingCurrency)
    {
        return PlayerPrefs.GetInt(KeyCurrency, defaultValue);
    }

    public void CashOutOnExtract()
    {
        CashOutPending();
    }

    public void CashOutOnDeath()
    {
        CashOutPending();
    }

    void CashOutPending()
    {
        if (pendingKills <= 0 && pendingEarnings <= 0)
            return;

        LastRunKills = pendingKills;
        LastRunMoneyEarned = pendingEarnings;

        pendingKills = 0;
        pendingEarnings = 0;
        SavePendingOnly();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(KeyCurrency))
            _currency = PlayerPrefs.GetInt(KeyCurrency, startingCurrency);
        else
            _currency = startingCurrency;

        pendingKills = PlayerPrefs.GetInt(KeyPendingKills, 0);
        pendingEarnings = PlayerPrefs.GetInt(KeyPendingEarnings, 0);
    }

    void SaveCurrencyOnly()
    {
        PlayerPrefs.SetInt(KeyCurrency, _currency);
        PlayerPrefs.Save();
    }

    void SavePendingOnly()
    {
        PlayerPrefs.SetInt(KeyPendingKills, pendingKills);
        PlayerPrefs.SetInt(KeyPendingEarnings, pendingEarnings);
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveCurrencyOnly();
            SavePendingOnly();
        }
    }

    void OnApplicationQuit()
    {
        SaveCurrencyOnly();
        SavePendingOnly();
    }
}
