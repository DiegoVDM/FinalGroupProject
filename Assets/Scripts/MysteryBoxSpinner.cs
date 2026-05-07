using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class MysteryBoxSpinner : MonoBehaviour
{
    [Header("UI References")]
    public GameObject spinPanel;           // Canvas panel that appears during spin
    public TextMeshProUGUI resultLabel;    // Shows the spin state and final perk name
    public Image resultIcon;              // Shows spinning and final perk icon

    [Header("Spin Settings")]
    public float spinDuration = 3f;       // Total spin time

    [Header("Perk Visuals")]
    public Sprite doubleHealthSprite;
    public Sprite speedSprite;
    public Sprite damageSprite;

    class CoroutineHost : MonoBehaviour { }
    static CoroutineHost _coroutineHost;
    static CoroutineHost Executor
    {
        get
        {
            if (_coroutineHost != null)
                return _coroutineHost;

            GameObject hostGO = new GameObject("MysteryBoxSpinner_CoroutineHost");
            UnityEngine.Object.DontDestroyOnLoad(hostGO);
            _coroutineHost = hostGO.AddComponent<CoroutineHost>();
            return _coroutineHost;
        }
    }

    public void HideSpinPanel()
    {
        if (spinPanel != null)
            spinPanel.SetActive(false);
        if (resultLabel != null)
            resultLabel.gameObject.SetActive(false);
    }

    // Called by MysteryBox
    public void PlaySpin(PerkType result, Action<PerkType> onComplete)
    {
        if (spinPanel != null && !spinPanel.activeInHierarchy)
            spinPanel.SetActive(true);

        Executor.StartCoroutine(SpinRoutine(result, onComplete));
    }

    IEnumerator SpinRoutine(PerkType result, Action<PerkType> onComplete)
    {
        if (spinPanel != null)
            spinPanel.SetActive(true);

        if (resultLabel != null)
        {
            resultLabel.gameObject.SetActive(true);
            resultLabel.text = "SPINNING...";
        }
        if (resultIcon != null)
        {
            resultIcon.gameObject.SetActive(true);
            resultIcon.rectTransform.localScale = Vector3.one * 2f;
            resultIcon.rectTransform.sizeDelta = new Vector2(260f, 260f);
        }

        Sprite[] icons = { doubleHealthSprite, speedSprite, damageSprite };
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            bool isLastFrame = elapsed + Mathf.Lerp(0.05f, 0.18f, elapsed / spinDuration) >= spinDuration;
            if (resultIcon != null)
            {
                if (isLastFrame)
                {
                    switch (result)
                    {
                        case PerkType.DoubleHealth:
                            resultIcon.sprite = doubleHealthSprite;
                            break;
                        case PerkType.Speed:
                            resultIcon.sprite = speedSprite;
                            break;
                        case PerkType.StrongerDamage:
                            resultIcon.sprite = damageSprite;
                            break;
                    }
                }
                else
                {
                    resultIcon.sprite = icons[UnityEngine.Random.Range(0, icons.Length)];
                }
            }

            float wait = Mathf.Lerp(0.05f, 0.18f, elapsed / spinDuration);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }

        if (resultIcon != null)
        {
            resultIcon.rectTransform.localScale = Vector3.one * 2f;
            resultIcon.rectTransform.sizeDelta = new Vector2(260f, 260f);
        }

        yield return new WaitForSeconds(1f);

        onComplete?.Invoke(result);
    }
}