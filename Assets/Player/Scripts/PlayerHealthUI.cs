using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthText;
    [SerializeField] private float fillSmoothSpeed = 8f;

    private RectTransform healthFillRect;
    private bool hasWarnedAboutMissingReferences;
    private float displayedHealthNormalized = 1f;

    void Awake()
    {
        if (healthFill != null)
        {
            healthFillRect = healthFill.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        if (playerHealth == null || healthFill == null || healthFillRect == null)
        {
            WarnAboutMissingReferences();
            return;
        }

        displayedHealthNormalized = Mathf.MoveTowards(
            displayedHealthNormalized,
            playerHealth.HealthNormalized,
            fillSmoothSpeed * Time.deltaTime);

        displayedHealthNormalized = Mathf.Clamp01(displayedHealthNormalized);

        healthFillRect.anchorMin = new Vector2(0f, 0f);
        healthFillRect.anchorMax = new Vector2(displayedHealthNormalized, 1f);
        healthFillRect.offsetMin = new Vector2(2f, 2f);
        healthFillRect.offsetMax = new Vector2(-2f, -2f);

        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.CeilToInt(playerHealth.CurrentHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}";
        }
    }

    void WarnAboutMissingReferences()
    {
        if (hasWarnedAboutMissingReferences)
            return;

        Debug.LogWarning("PlayerHealthUI is missing PlayerHealth, HealthFill Image, or HealthFill RectTransform reference.");
        hasWarnedAboutMissingReferences = true;
    }
}