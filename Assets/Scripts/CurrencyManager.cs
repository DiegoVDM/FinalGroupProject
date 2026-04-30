using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] private int startingCurrency = 500;
    public UnityEvent<int> onCurrencyChanged;

    int _currency;
    public int Currency => _currency;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _currency = startingCurrency;
    }

    public bool TrySpend(int amount)
    {
        if (_currency < amount) return false;
        _currency -= amount;
        onCurrencyChanged?.Invoke(_currency);
        return true;
    }

    public void Add(int amount)
    {
        _currency += amount;
        onCurrencyChanged?.Invoke(_currency);
    }

    //don't exist yet, just examples of how we might want to give money based on kills and extraction

    // public void AwardExtraction(int killCount)
    // {
    //     Add( killCount * 25 * 2);
    // }

    // public void AwardDeath(int killCount)
    // {
    //     Add(killCount * 25);
    // }
}