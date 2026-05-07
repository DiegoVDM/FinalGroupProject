using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class PinchZoomCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float minOrthographicSize = 7f;
    [SerializeField] private float maxOrthographicSize = 18f;
    [SerializeField] private float pinchZoomSpeed = 0.03f;
    [SerializeField] private bool enableMouseWheelTesting = true;
    [SerializeField] private float mouseWheelZoomSpeed = 3f;
    [SerializeField] private bool ignorePinchOverUI = true;

    private void Reset()
    {
        AssignCameraReference();
    }

    private void Awake()
    {
        AssignCameraReference();
        ClampCurrentZoom();
    }

    private void OnValidate()
    {
        if (minOrthographicSize > maxOrthographicSize)
        {
            float previousMin = minOrthographicSize;
            minOrthographicSize = maxOrthographicSize;
            maxOrthographicSize = previousMin;
        }

        minOrthographicSize = Mathf.Max(0.1f, minOrthographicSize);
        maxOrthographicSize = Mathf.Max(minOrthographicSize, maxOrthographicSize);
    }

    private void Update()
    {
        HandleMouseWheelZoom();
        HandlePinchZoom();
    }

    private void HandleMouseWheelZoom()
    {
        if (!enableMouseWheelTesting || targetCamera == null)
            return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollDelta) <= Mathf.Epsilon)
            return;

        ApplyZoomDelta(-scrollDelta * mouseWheelZoomSpeed);
    }

    private void HandlePinchZoom()
    {
        if (targetCamera == null || Input.touchCount != 2)
            return;

        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        if (ignorePinchOverUI && IsTouchOverUI(firstTouch.fingerId, secondTouch.fingerId))
            return;

        Vector2 firstPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

        float previousDistance = Vector2.Distance(firstPreviousPosition, secondPreviousPosition);
        float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
        float pinchDelta = currentDistance - previousDistance;

        if (Mathf.Abs(pinchDelta) <= Mathf.Epsilon)
            return;

        ApplyZoomDelta(pinchDelta * pinchZoomSpeed);
    }

    private bool IsTouchOverUI(int firstFingerId, int secondFingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(firstFingerId)
            || EventSystem.current.IsPointerOverGameObject(secondFingerId);
    }

    private void ApplyZoomDelta(float zoomDelta)
    {
        targetCamera.orthographicSize = Mathf.Clamp(
            targetCamera.orthographicSize + zoomDelta,
            minOrthographicSize,
            maxOrthographicSize);
    }

    private void ClampCurrentZoom()
    {
        if (targetCamera == null)
            return;

        targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize, minOrthographicSize, maxOrthographicSize);
    }

    private void AssignCameraReference()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }
}
