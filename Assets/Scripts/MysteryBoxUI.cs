using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MysteryBoxUI : MonoBehaviour
{
    [Header("Cost")]
    public int spinCost = 100;

    [Header("Buttons & Labels")]
    public Button spinButton;
    public MysteryBoxSpinner spinner;
    public TextMeshProUGUI balanceLabel;
    public TextMeshProUGUI spinButtonLabel;

    [Header("Active Perk Display")]
    public TextMeshProUGUI activePerkLabel;
    public Image activePerkIcon;

    [Header("Reveal Panel")]
    public GameObject revealPanel;       
    public Image revealIcon;        // Perk Icon
    public TextMeshProUGUI revealTitle;      // Perk Title like "2x HEALTH
    public Button closeRevealButton;

    [Header("Perk Icons")]
    public Sprite healthSprite;
    public Sprite speedSprite;
    public Sprite damageSprite;

    [Header("Mystery Box (World)")]
    public MysteryBoxController mysteryBox;

    [Header("Animation")]
    public float punchScale    = 1.00f;  // icon size
    public float punchDuration = 0.18f;
    public float settleDuration = 0.12f;
    bool _spinning = false;

    void Awake()
    {
        ResolveMissingReferences();
    }

    void Start()
    {
        if (revealPanel != null)
            revealPanel.SetActive(false);

        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinPressed);
        }

        if (closeRevealButton != null)
        {
            closeRevealButton.onClick.RemoveAllListeners();
            closeRevealButton.onClick.AddListener(HideRevealPanel);
        }

        RefreshBalance();
        HideRevealPanel();

        // keep balance label live if currency changes elsewhere
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.onCurrencyChanged.AddListener(_ => RefreshBalance());

        // initialize + keep active perk label live
        RefreshActivePerkDisplay();
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.ActivePerkChanged += _ => RefreshActivePerkDisplay();
    }

    void ResolveMissingReferences()
    {
        if (spinButton == null)
            spinButton = FindComponentInScene<Button>("SpinButton");

        if (spinner == null)
            spinner = Object.FindAnyObjectByType<MysteryBoxSpinner>();

        if (mysteryBox == null)
            mysteryBox = Object.FindAnyObjectByType<MysteryBoxController>();

        if (balanceLabel == null)
            balanceLabel = FindComponentInScene<TextMeshProUGUI>("Balance");

        if (spinButtonLabel == null && spinButton != null)
            spinButtonLabel = spinButton.GetComponentInChildren<TextMeshProUGUI>();

        if (closeRevealButton == null)
            closeRevealButton = FindComponentInScene<Button>("CloseButton");

        if (revealPanel == null && closeRevealButton != null)
            revealPanel = closeRevealButton.transform.parent?.gameObject;

        if (revealPanel == null)
            revealPanel = GameObject.Find("RevealPanel");

        if (revealIcon == null)
            revealIcon = FindComponentInScene<Image>("RevealIcon") ?? FindChildImageInPanel(revealPanel);

        if (revealTitle == null)
            revealTitle = FindComponentInScene<TextMeshProUGUI>("RevealTitle") ?? FindChildTextInPanel(revealPanel);

        if (activePerkLabel == null)
            activePerkLabel = FindComponentInScene<TextMeshProUGUI>("ActivePerkText");

        if (activePerkIcon == null)
            activePerkIcon = FindComponentInScene<Image>("ActivePerkIcon");
    }

    T FindComponentInScene<T>(string objectName) where T : Component
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<T>() : null;
    }

    Image FindChildImageInPanel(GameObject panel)
    {
        if (panel == null) return null;
        foreach (Image image in panel.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject == panel) continue;
            if (image.GetComponent<Button>() != null) continue;
            return image;
        }
        return null;
    }

    TextMeshProUGUI FindChildTextInPanel(GameObject panel)
    {
        if (panel == null) return null;
        foreach (TextMeshProUGUI text in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.gameObject == spinButton?.gameObject) continue;
            if (text.gameObject == closeRevealButton?.gameObject) continue;
            return text;
        }
        return null;
    }

    void RefreshBalance()
    {
        if (CurrencyManager.Instance == null) return;
        if (balanceLabel != null)
            balanceLabel.text = $"Balance: ${CurrencyManager.Instance.Currency}";
        if (spinButtonLabel != null)
            spinButtonLabel.text = $"Spin  —  ${spinCost}";
        if (spinButton != null)
            spinButton.interactable = CurrencyManager.Instance.Currency >= spinCost && !_spinning;
    }

    void RefreshActivePerkDisplay()
    {
        if (activePerkLabel == null && activePerkIcon == null)
            return;

        PerkState.EnsureLoaded();
        if (PerkState.ActivePerk == null)
        {
            if (activePerkLabel != null)
                activePerkLabel.text = "Active Perk: None";
            if (activePerkIcon != null)
                activePerkIcon.enabled = false;
            return;
        }

        PerkType perk = PerkState.ActivePerk.Value;
        if (activePerkLabel != null)
            activePerkLabel.text = $"Active Perk: {PerkToDisplayName(perk)}";

        if (activePerkIcon != null)
        {
            activePerkIcon.enabled = true;
            activePerkIcon.sprite = perk switch
            {
                PerkType.DoubleHealth => healthSprite,
                PerkType.Speed => speedSprite,
                PerkType.StrongerDamage => damageSprite,
                _ => activePerkIcon.sprite
            };
        }
    }

    static string PerkToDisplayName(PerkType perk)
    {
        return perk switch
        {
            PerkType.DoubleHealth => "Double Health",
            PerkType.Speed => "Speed Boost",
            PerkType.StrongerDamage => "Double Tap",
            _ => perk.ToString()
        };
    }

    void HideRevealPanel()
    {
        if (revealPanel != null)
            revealPanel.SetActive(false);
        if (closeRevealButton != null)
            closeRevealButton.gameObject.SetActive(false);
        if (spinner != null)
            spinner.HideSpinPanel();
    }

    // Spin 
    void OnSpinPressed()
    {
        if (_spinning) return;
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpend(spinCost))
        {
            StartCoroutine(ShakeButton());
            return;
        }

        if (mysteryBox != null)
            mysteryBox.Spin();

        PerkType result = (PerkType)Random.Range(0, 3); // equal chance for now, we can change based on which is more op
        _spinning = true;
        if (spinButton != null)
            spinButton.interactable = false;

        if (spinner != null)
        {
            spinner.PlaySpin(result, r => StartCoroutine(RevealRoutine(r)));
        }
        else
        {
            StartCoroutine(RevealRoutine(result));
        }
    }

    IEnumerator RevealRoutine(PerkType perk)
    {
        _spinning = true;
        if (spinButton != null)
            spinButton.interactable = false;

        if (revealPanel != null)
            revealPanel.SetActive(true);
        if (closeRevealButton != null)
            closeRevealButton.gameObject.SetActive(true);
        if (revealIcon != null)
        {
            revealIcon.rectTransform.localScale = Vector3.one * 2f;
            revealIcon.rectTransform.sizeDelta = new Vector2(260f, 260f);
        }

        // Directly show the final perk without a second animation cycle.
        switch (perk)
        {
            case PerkType.DoubleHealth:
                if (revealIcon != null) revealIcon.sprite = healthSprite;
                if (revealTitle != null) revealTitle.text = "Double Health";
                break;
            case PerkType.Speed:
                if (revealIcon != null) revealIcon.sprite = speedSprite;
                if (revealTitle != null) revealTitle.text = "Speed Boost";
                break;
            case PerkType.StrongerDamage:
                if (revealIcon != null) revealIcon.sprite = damageSprite;
                if (revealTitle != null) revealTitle.text = "Double Tap";
                break;
        }

        // Animates the icon with a small effect
        if (revealIcon != null)
            yield return StartCoroutine(PunchIcon(revealIcon.transform));

        // Applies the perk
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.ApplyPerk(perk);
        else
            PerkState.Apply(perk);

        RefreshActivePerkDisplay();

        _spinning = false;
        RefreshBalance();
        if (spinButton != null)
            spinButton.interactable = CurrencyManager.Instance != null && CurrencyManager.Instance.Currency >= spinCost && !_spinning;
    }

    // simple animation that makes the icon a little bigger then goes back to normal
    IEnumerator PunchIcon(Transform t)
    {
        Vector3 startScale = t.localScale;
        Vector3 targetScale = startScale * punchScale;

        // Scale up
        float e = 0f;
        while (e < punchDuration)
        {
            e += Time.deltaTime;
            t.localScale = Vector3.Lerp(startScale, targetScale, e / punchDuration);
            yield return null;
        }
        // goes back
        e = 0f;
        while (e < settleDuration)
        {
            e += Time.deltaTime;
            t.localScale = Vector3.Lerp(targetScale, startScale, e / settleDuration);
            yield return null;
        }
        t.localScale = startScale;
    }

    //  Shakes the button if you can't afford it 
    IEnumerator ShakeButton()
    {
        Vector3 origin = spinButton.transform.localPosition;
        float[] offsets = { -18f, 18f, -12f, 12f, -6f, 6f, 0f };
        foreach (float x in offsets)
        {
            spinButton.transform.localPosition = origin + new Vector3(x, 0f, 0f);
            yield return new WaitForSeconds(0.04f);
        }
        spinButton.transform.localPosition = origin;
    }
}