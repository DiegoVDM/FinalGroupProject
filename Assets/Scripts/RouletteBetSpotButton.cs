using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RouletteBetSpotButton : MonoBehaviour
{
    public enum SpotKind { Number, Color }

    [Header("Spot")]
    [SerializeField] SpotKind kind = SpotKind.Number;
    [SerializeField] int number = 0;
    [SerializeField] RouletteGame.RouletteColor color = RouletteGame.RouletteColor.Red;

    [Header("UI")]
    [SerializeField] Button button;
    [SerializeField] TMP_Text labelText;
    [SerializeField] TMP_Text betText;
    [SerializeField] Image background;

    UnityAction _onClick;
    string _baseLabel = string.Empty;

    void Reset()
    {
        button = GetComponent<Button>();
        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>();
    }

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (background == null)
            background = GetComponent<Image>();
    }

    public void ConfigureAsNumber(int n, RouletteGame.RouletteColor numberColor)
    {
        kind = SpotKind.Number;
        number = n;

        _baseLabel = n.ToString();
        if (labelText != null)
            labelText.text = _baseLabel;

        ApplyNumberColor(numberColor);
    }

    public void ConfigureAsColor(RouletteGame.RouletteColor c)
    {
        kind = SpotKind.Color;
        color = c;

        _baseLabel = c.ToString();
        if (labelText != null)
            labelText.text = _baseLabel;
    }

    void ApplyNumberColor(RouletteGame.RouletteColor numberColor)
    {
        if (background != null)
        {
            background.color = numberColor switch
            {
                RouletteGame.RouletteColor.Red => new Color(0.75f, 0.15f, 0.15f, 1f),
                RouletteGame.RouletteColor.Black => new Color(0.12f, 0.12f, 0.12f, 1f),
                _ => new Color(0.10f, 0.50f, 0.20f, 1f), // green for 0
            };
        }

        // Ensure text stays readable on dark backgrounds
        if (labelText != null)
            labelText.color = Color.white;
        if (betText != null)
            betText.color = Color.white;
    }

    public void SetClickHandler(UnityAction onClick)
    {
        _onClick = onClick;
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        _onClick?.Invoke();
    }

    public void SetBetAmount(int amount)
    {
        if (betText != null)
            betText.text = amount > 0 ? $"${amount}" : string.Empty;

        // When a bet is placed, hide the base label (number/color) so the bet is clear.
        if (labelText != null)
        {
            labelText.text = amount > 0 ? string.Empty : _baseLabel;
            labelText.enabled = amount <= 0;
        }
    }

    public string GetBetKey()
    {
        return kind switch
        {
            SpotKind.Number => $"N:{number}",
            SpotKind.Color => $"C:{color}",
            _ => string.Empty
        };
    }

    public bool TryGetNumber(out int n)
    {
        if (kind == SpotKind.Number)
        {
            n = number;
            return true;
        }

        n = -1;
        return false;
    }
}

