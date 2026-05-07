using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class RouletteGame : MonoBehaviour
{
    public enum RouletteColor { Red, Black, Green }

    [Header("UI References")]
    public TMP_InputField chipValueInput;

    public RectTransform numberGridParent;

    public RouletteBetSpotButton betSpotButtonPrefab;

    public Button redBetButton;

    public Button blackBetButton;

    public Button clearBetsButton;

    public Button spinButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI walletText;
    public TextMeshProUGUI payoutText;

    [Header("Roulette Settings")]
    public int minimumBet = 10;

    public bool autoCreateNumberButtons = true;

    readonly Dictionary<string, int> _bets = new Dictionary<string, int>();
    readonly Dictionary<string, RouletteBetSpotButton> _betSpotButtons = new Dictionary<string, RouletteBetSpotButton>();
    int _chipValue = 10;

    static readonly RouletteColor[] NumberColors = new RouletteColor[]
    {
        RouletteColor.Green, // 0
        RouletteColor.Red,   // 1
        RouletteColor.Black, // 2
        RouletteColor.Red,   // 3
        RouletteColor.Black, // 4
        RouletteColor.Red,   // 5
        RouletteColor.Black, // 6
        RouletteColor.Red,   // 7
        RouletteColor.Black, // 8
        RouletteColor.Red,   // 9
        RouletteColor.Black, // 10
        RouletteColor.Black, // 11
        RouletteColor.Red,   // 12
        RouletteColor.Black, // 13
        RouletteColor.Red,   // 14
        RouletteColor.Black, // 15
        RouletteColor.Red,   // 16
        RouletteColor.Black, // 17
        RouletteColor.Red,   // 18
        RouletteColor.Red,   // 19
        RouletteColor.Black, // 20
        RouletteColor.Red,   // 21
        RouletteColor.Black, // 22
        RouletteColor.Red,   // 23
        RouletteColor.Black, // 24
        RouletteColor.Red,   // 25
        RouletteColor.Black, // 26
        RouletteColor.Red,   // 27
        RouletteColor.Black, // 28
        RouletteColor.Black, // 29
        RouletteColor.Red,   // 30
        RouletteColor.Black, // 31
        RouletteColor.Red,   // 32
        RouletteColor.Black, // 33
        RouletteColor.Red,   // 34
        RouletteColor.Black, // 35
        RouletteColor.Red    // 36
    };

    void Start()
    {
        HookUi();
        ReadChipValueFromInputOrDefault();
        BuildOrIndexBetButtons();

        UpdateWalletDisplay();
        UpdateBetDisplays();
        ShowResult("Click numbers to add bets. You can bet multiple spots (ex: Red + 17).");
    }

    void HookUi()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(SpinRoulette);

        if (redBetButton != null)
            redBetButton.onClick.AddListener(() => AddColorBet(RouletteColor.Red, GetChipValue()));

        if (blackBetButton != null)
            blackBetButton.onClick.AddListener(() => AddColorBet(RouletteColor.Black, GetChipValue()));

        if (clearBetsButton != null)
            clearBetsButton.onClick.AddListener(() => ClearAllBets(showMessage: true));

        if (chipValueInput != null)
            chipValueInput.onEndEdit.AddListener(_ => ReadChipValueFromInputOrDefault());
    }

    void UpdateWalletDisplay()
    {
        if (walletText == null) return;
        walletText.text = CurrencyManager.Instance != null
            ? $"Wallet: ${CurrencyManager.Instance.Currency}"
            : "Wallet: $0";
    }

    void UpdateBetDisplays()
    {
        if (payoutText == null) return;

        if (!payoutText.gameObject.activeInHierarchy)
            payoutText.gameObject.SetActive(true);

        int totalBet = GetTotalBet();
        payoutText.text = $"Chip: ${GetChipValue()} | Total bet: ${totalBet}";
    }

    void ReadChipValueFromInputOrDefault()
    {
        int parsed;
        if (chipValueInput == null || !int.TryParse(chipValueInput.text, out parsed) || parsed <= 0)
        {
            _chipValue = Mathf.Max(1, minimumBet);
            if (chipValueInput != null)
                chipValueInput.text = _chipValue.ToString();
            return;
        }

        _chipValue = parsed;
    }

    int GetChipValue()
    {
        if (_chipValue <= 0)
            _chipValue = Mathf.Max(1, minimumBet);
        return _chipValue;
    }

    void BuildOrIndexBetButtons()
    {
        _betSpotButtons.Clear();

        if (autoCreateNumberButtons)
        {
            if (numberGridParent == null || betSpotButtonPrefab == null)
            {
                Debug.LogWarning("RouletteGame: autoCreateNumberButtons is ON but numberGridParent or betSpotButtonPrefab is not set.");
                return;
            }

            for (int n = 0; n <= 36; n++)
            {
                int number = n; // capture per-iteration value for button callbacks
                var btn = Instantiate(betSpotButtonPrefab, numberGridParent);
                btn.ConfigureAsNumber(number, GetColorForNumber(number));
                btn.SetClickHandler(() => AddNumberBet(number, GetChipValue()));
                RegisterBetSpotButton(btn);
            }
        }
        else
        {
            // If you prefer to build buttons manually in the Scene,
            // we’ll index any RouletteBetSpotButton children and wire them.
            if (numberGridParent == null) return;
            var children = numberGridParent.GetComponentsInChildren<RouletteBetSpotButton>(true);
            foreach (var btn in children)
            {
                var capturedBtn = btn;
                btn.SetClickHandler(() =>
                {
                    if (capturedBtn != null && capturedBtn.TryGetNumber(out int number))
                        AddNumberBet(number, GetChipValue());
                });
                RegisterBetSpotButton(btn);
            }
        }
    }

    void RegisterBetSpotButton(RouletteBetSpotButton btn)
    {
        if (btn == null) return;
        string key = btn.GetBetKey();
        if (string.IsNullOrWhiteSpace(key)) return;
        _betSpotButtons[key] = btn;
        btn.SetBetAmount(GetBetAmount(key));
    }

    public void AddNumberBet(int number, int amount)
    {
        if (number < 0 || number > 36) return;
        if (amount <= 0) return;
        AddToBet(Key_Number(number), amount);
        ShowResult($"Bet +${amount} on {number}");
    }

    public void AddColorBet(RouletteColor color, int amount)
    {
        if (color == RouletteColor.Green) return; // no green outside bet here
        if (amount <= 0) return;
        AddToBet(Key_Color(color), amount);
        ShowResult($"Bet +${amount} on {color}");
    }

    void AddToBet(string key, int amount)
    {
        int next = GetBetAmount(key) + amount;
        _bets[key] = next;

        if (_betSpotButtons.TryGetValue(key, out var btn) && btn != null)
            btn.SetBetAmount(next);

        UpdateBetDisplays();
        UpdateWalletDisplay();
    }

    void ClearAllBets(bool showMessage = true)
    {
        _bets.Clear();
        foreach (var kvp in _betSpotButtons)
        {
            if (kvp.Value != null)
                kvp.Value.SetBetAmount(0);
        }
        UpdateBetDisplays();
        if (showMessage)
            ShowResult("Bets cleared.");
    }

    int GetBetAmount(string key)
    {
        return _bets.TryGetValue(key, out int v) ? v : 0;
    }

    int GetTotalBet()
    {
        int total = 0;
        foreach (var kvp in _bets)
            total += Mathf.Max(0, kvp.Value);
        return total;
    }

    void SpinRoulette()
    {
        if (CurrencyManager.Instance == null)
        {
            ShowResult("Currency manager missing.");
            return;
        }

        int totalBet = GetTotalBet();
        if (totalBet <= 0)
        {
            ShowResult("Place at least one bet.");
            return;
        }

        if (totalBet < minimumBet)
        {
            ShowResult($"Minimum total bet is ${minimumBet}.");
            return;
        }

        if (totalBet > CurrencyManager.Instance.Currency)
        {
            ShowResult("Not enough money for those bets.");
            return;
        }

        if (!CurrencyManager.Instance.TrySpend(totalBet))
        {
            ShowResult("Unable to place bets. Not enough funds.");
            return;
        }

        int resultNumber = Random.Range(0, 37);
        RouletteColor resultColor = GetColorForNumber(resultNumber);

        int payout = CalculatePayout(resultNumber, resultColor, out string winBreakdown);
        if (payout > 0)
            CurrencyManager.Instance.Add(payout);

        var sb = new StringBuilder();
        sb.Append($"Landed on: {resultNumber} {resultColor}");
        sb.Append($"\nBet: ${totalBet} | Payout: ${payout}");
        if (!string.IsNullOrWhiteSpace(winBreakdown))
            sb.Append($"\nWins: {winBreakdown}");

        ShowResult(sb.ToString());
        UpdateWalletDisplay();
        ClearAllBets(showMessage: false);
    }

    RouletteColor GetColorForNumber(int number)
    {
        if (number < 0 || number > 36)
            return RouletteColor.Green;

        return NumberColors[number];
    }

    int CalculatePayout(int resultNumber, RouletteColor resultColor, out string winBreakdown)
    {
        int payout = 0;
        var wins = new List<string>();
        
        //number bet
        string numberKey = Key_Number(resultNumber);
        int numberBet = GetBetAmount(numberKey);
        if (numberBet > 0)
        {
            int won = numberBet * 36;
            payout += won;
            wins.Add($"{resultNumber} pays ${won}");
        }

        // Color bet
        if (resultColor == RouletteColor.Red || resultColor == RouletteColor.Black)
        {
            string colorKey = Key_Color(resultColor);
            int colorBet = GetBetAmount(colorKey);
            if (colorBet > 0)
            {
                int won = colorBet * 2;
                payout += won;
                wins.Add($"{resultColor} pays ${won}");
            }
        }

        winBreakdown = string.Join(", ", wins);
        return payout;
    }

    static string Key_Number(int number) => $"N:{number}";
    static string Key_Color(RouletteColor color) => $"C:{color}";

    void ShowResult(string message)
    {
        if (resultText != null)
        {
            if (!resultText.gameObject.activeInHierarchy)
                resultText.gameObject.SetActive(true);
            resultText.text = message;
        }
        Debug.Log(message);
    }
}
