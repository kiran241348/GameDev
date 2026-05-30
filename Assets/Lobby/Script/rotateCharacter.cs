using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollToRotateCharacter : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Character Settings")]
    public Transform characterToRotate;
    public float rotationSpeed = 5f;
    public bool smoothRotation = true;
    public float smoothTime = 0.1f;

    [Header("Scroll Settings")]
    public RectTransform scrollArea; // The image area to scroll on
    public bool invertRotation = false;

    [Header("UI Settings")]
    public bool allowOtherUI = true; // Allow other UI buttons to work

    [Header("Rotation Limits")]
    public bool limitRotation = false;
    public float minAngle = -180f;
    public float maxAngle = 180f;

    [Header("Visual Feedback")]
    public bool showDebugLogs = true;
    public GameObject rotationRingEffect;

    [Header("Emote System Integration")]
    public LobbyEmoteSystem emoteSystem; // Reference to the emote system

    private float targetRotationY = 0f;
    private float currentRotationY = 0f;
    private float rotationVelocity = 0f;
    private bool isDragging = false;
    private Vector2 lastScrollPosition;
    private bool isOverScrollArea = false;

    private void Start()
    {
        if (characterToRotate == null)
            characterToRotate = transform;

        currentRotationY = characterToRotate.eulerAngles.y;
        targetRotationY = currentRotationY;

        if (scrollArea == null)
        {
            Image img = GetComponent<Image>();
            if (img != null)
                scrollArea = GetComponent<RectTransform>();
        }

        // Find emote system if not assigned
        if (emoteSystem == null)
        {
            emoteSystem = FindObjectOfType<LobbyEmoteSystem>();
        }

        if (showDebugLogs)
            Debug.Log("Scroll Rotate System Initialized");
    }

    private void Update()
    {
        // Handle mouse scroll wheel only when over scroll area
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0 && IsMouseOverScrollArea())
        {
            OnScroll(scrollDelta);
        }

        // Apply smooth rotation
        if (smoothRotation)
        {
            currentRotationY = Mathf.SmoothDamp(currentRotationY, targetRotationY, ref rotationVelocity, smoothTime);
            ApplyRotation(currentRotationY);
        }
        else
        {
            ApplyRotation(targetRotationY);
        }
    }

    private bool IsMouseOverScrollArea()
    {
        if (scrollArea == null) return true;

        Vector2 mousePos = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(scrollArea, mousePos);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!IsPointerOverScrollArea(eventData) && allowOtherUI) return;

        float scrollDelta = eventData.scrollDelta.y;
        OnScroll(scrollDelta);
    }

    public void OnScroll(float scrollDelta)
    {
        if (characterToRotate == null) return;

        float rotationAmount = -scrollDelta * rotationSpeed;

        if (invertRotation)
            rotationAmount = -rotationAmount;

        targetRotationY += rotationAmount;

        if (limitRotation)
            targetRotationY = Mathf.Clamp(targetRotationY, minAngle, maxAngle);

        if (showDebugLogs)
            Debug.Log($"Scrolling: {scrollDelta}, Rotation: {targetRotationY}");

        if (rotationRingEffect != null)
        {
            rotationRingEffect.SetActive(true);
            CancelInvoke(nameof(HideRotationEffect));
            Invoke(nameof(HideRotationEffect), 0.5f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsPointerOverScrollArea(eventData) && allowOtherUI) return;

        lastScrollPosition = eventData.position;
        isDragging = true;

        // Notify emote system that rotation has started
        if (emoteSystem != null)
        {
            emoteSystem.SetRotating(true);
            if (showDebugLogs)
                Debug.Log("Notified emote system: Rotation started");
        }

        if (showDebugLogs)
            Debug.Log("Started dragging for rotation");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 dragDelta = eventData.position - lastScrollPosition;

        float rotationAmount = dragDelta.x * (rotationSpeed * 0.1f);

        if (invertRotation)
            rotationAmount = -rotationAmount;

        targetRotationY += rotationAmount;

        if (limitRotation)
            targetRotationY = Mathf.Clamp(targetRotationY, minAngle, maxAngle);

        lastScrollPosition = eventData.position;

        if (!smoothRotation)
            ApplyRotation(targetRotationY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Notify emote system that rotation has ended
        if (emoteSystem != null)
        {
            emoteSystem.SetRotating(false);
            if (showDebugLogs)
                Debug.Log("Notified emote system: Rotation ended");
        }

        if (showDebugLogs)
            Debug.Log("Stopped dragging");
    }

    private bool IsPointerOverScrollArea(PointerEventData eventData)
    {
        if (scrollArea == null) return true;
        return RectTransformUtility.RectangleContainsScreenPoint(scrollArea, eventData.position);
    }

    private void ApplyRotation(float yRotation)
    {
        Vector3 newRotation = characterToRotate.eulerAngles;
        newRotation.y = yRotation;
        characterToRotate.eulerAngles = newRotation;
    }

    private void HideRotationEffect()
    {
        if (rotationRingEffect != null)
            rotationRingEffect.SetActive(false);
    }

    public void ResetRotation()
    {
        targetRotationY = 0f;
        currentRotationY = 0f;
        ApplyRotation(0f);
    }

    public void SetRotation(float angle)
    {
        targetRotationY = angle;
        currentRotationY = angle;
        ApplyRotation(angle);
    }

    // Public method to check if currently rotating
    public bool IsRotating()
    {
        return isDragging;
    }

    private void OnDisable()
    {
        // Reset rotation state when disabled
        if (emoteSystem != null && isDragging)
        {
            emoteSystem.SetRotating(false);
        }
        isDragging = false;
    }
}