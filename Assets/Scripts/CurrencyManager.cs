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

    const string KeyCurrency = "CurrencyManager.Currency";
    const string KeyPendingKills = "CurrencyManager.PendingKills";
    const string KeyPendingEarnings = "CurrencyManager.PendingEarnings";

    int _currency;
    public int Currency => _currency;
    public int PendingKills => pendingKills;
    public int PendingEarnings => pendingEarnings;

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
        pendingKills += kills;
        pendingEarnings += kills * moneyPerKill;
        SavePendingOnly();
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
        if (pendingEarnings > 0)
            Add(pendingEarnings);

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