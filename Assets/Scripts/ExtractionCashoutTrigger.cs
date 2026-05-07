using UnityEngine;

public class ExtractionCashoutTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.CashOutOnExtract();
    }
}
