using UnityEngine;

public class MuzzleFlashBillboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    public Camera targetCamera;
    public bool invertFacing = false;

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
            return;

        if (invertFacing)
        {
            transform.forward = -targetCamera.transform.forward;
        }
        else
        {
            transform.forward = targetCamera.transform.forward;
        }
    }
}