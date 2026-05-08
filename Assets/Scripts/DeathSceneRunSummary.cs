using TMPro;
using UnityEngine;

/// <summary>
/// Wire your death-scene TextMeshPro labels here. Shows kills and run earnings from the last
/// extract/death cash-out (see CurrencyManager.LastRunKills / LastRunMoneyEarned).
/// </summary>
public class DeathSceneRunSummary : MonoBehaviour
{
    [Header("Labels (optional — assign only what you use)")]
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI moneyEarnedText;
    [SerializeField] TextMeshProUGUI totalWalletText;

    [Header("Formatting — {0} = number")]
    [SerializeField] string killsFormat = "Kills: {0}";
    [SerializeField] string moneyEarnedFormat = "Earned this run: ${0}";
    [SerializeField] string totalWalletFormat = "Total cash: ${0}";

    [Header("Fallback if kills are not tracked only via CurrencyManager")]
    [Tooltip("If set, also consider DeadOpsDemoManager.kills (prototype demo).")]
    [SerializeField] bool preferDemoKillCountWhenHigher = true;

    [Tooltip("If LastRunMoneyEarned is 0 but kill count > 0, show kills × money-per-kill so the line matches the kill line (e.g. demo kills without wallet snapshot).")]
    [SerializeField] bool estimateMoneyFromKillsWhenEarnedIsZero = true;

    void Start()
    {
        Refresh();
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        int killsFromWallet = 0;
        int earned = 0;
        int wallet = 0;
        int perKill = 25;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.CashOutOnDeath();
            killsFromWallet = CurrencyManager.Instance.LastRunKills;
            earned = CurrencyManager.Instance.LastRunMoneyEarned;
            wallet = CurrencyManager.Instance.Currency;
            perKill = Mathf.Max(1, CurrencyManager.Instance.MoneyPerKill);
        }
        else
        {
            perKill = Mathf.Max(1, DeadOpsDemoManager.GetMoneyPerZombieKill());
        }

        int kills = killsFromWallet;
        if (preferDemoKillCountWhenHigher)
            kills = Mathf.Max(kills, DeadOpsDemoManager.kills);

        if (estimateMoneyFromKillsWhenEarnedIsZero && earned <= 0 && kills > 0)
            earned = kills * perKill;

        if (killsText != null)
        {
            killsText.gameObject.SetActive(true);
            killsText.text = string.Format(killsFormat, kills);
        }

        if (moneyEarnedText != null)
        {
            moneyEarnedText.gameObject.SetActive(true);
            moneyEarnedText.text = string.Format(moneyEarnedFormat, earned);
        }

        if (totalWalletText != null)
        {
            totalWalletText.gameObject.SetActive(true);
            totalWalletText.text = string.Format(totalWalletFormat, wallet);
        }
    }
}
