using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.1f;

    public Vector2 Direction { get; private set; }

    private float BackgroundRadius => background != null ? Mathf.Min(background.rect.width, background.rect.height) * 0.5f : 0f;
    private float HandleRadius => handle != null ? Mathf.Min(handle.rect.width, handle.rect.height) * 0.5f : 0f;
    private float MaxHandleDistance => Mathf.Max(0f, BackgroundRadius - HandleRadius);

    private void Reset()
    {
        AssignChildReferences();
    }

    private void OnValidate()
    {
        if (!IsOwnChild(background) || !IsOwnChild(handle))
        {
            AssignChildReferences();
        }
    }

    private void Awake()
    {
        CenterHandle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateJoystick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateJoystick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Direction = Vector2.zero;
        CenterHandle();
    }

    private void UpdateJoystick(PointerEventData eventData)
    {
        if (background == null || handle == null)
        {
            Direction = Vector2.zero;
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        float maxHandleDistance = MaxHandleDistance;
        Vector2 clampedPoint = Vector2.ClampMagnitude(localPoint, maxHandleDistance);
        handle.anchoredPosition = clampedPoint;

        if (maxHandleDistance <= 0f)
        {
            Direction = Vector2.zero;
            return;
        }

        Vector2 normalized = clampedPoint / maxHandleDistance;
        Direction = normalized.magnitude >= deadZone ? normalized : Vector2.zero;
    }

    private void CenterHandle()
    {
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void AssignChildReferences()
    {
        Transform backgroundChild = transform.Find("Background");
        Transform handleChild = transform.Find("Handle");

        background = backgroundChild as RectTransform;
        handle = handleChild as RectTransform;
    }

    private bool IsOwnChild(RectTransform rectTransform)
    {
        return rectTransform != null && rectTransform.parent == transform;
    }
}
