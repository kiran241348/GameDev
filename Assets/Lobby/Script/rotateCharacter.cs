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
    public ScrollRect scrollRect; // Optional: if using ScrollRect
    public RectTransform scrollArea; // The image area to scroll on
    public bool invertRotation = false; // Set to true if you want to reverse direction

    [Header("Rotation Limits")]
    public bool limitRotation = false;
    public float minAngle = -180f;
    public float maxAngle = 180f;

    [Header("Visual Feedback")]
    public bool showDebugLogs = true;
    public GameObject rotationRingEffect;

    private float targetRotationY = 0f;
    private float currentRotationY = 0f;
    private float rotationVelocity = 0f;
    private bool isScrolling = false;
    private Vector2 lastScrollPosition;

    private void Start()
    {
        if (characterToRotate == null)
            characterToRotate = transform;

        currentRotationY = characterToRotate.eulerAngles.y;
        targetRotationY = currentRotationY;

        // Setup scroll area if not assigned
        if (scrollArea == null)
        {
            Image img = GetComponent<Image>();
            if (img != null)
                scrollArea = GetComponent<RectTransform>();
        }

        if (showDebugLogs)
            Debug.Log("Scroll Rotate System Initialized");
    }

    private void Update()
    {
        // Handle mouse scroll wheel
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
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

    public void OnScroll(PointerEventData eventData)
    {
        // Get scroll delta from event
        float scrollDelta = eventData.scrollDelta.y;
        OnScroll(scrollDelta);
    }

    public void OnScroll(float scrollDelta)
    {
        if (characterToRotate == null) return;

        isScrolling = true;

        // FIXED: Calculate rotation amount - Natural direction (scroll up = rotate right)
        float rotationAmount = -scrollDelta * rotationSpeed;

        // Apply inversion if needed
        if (invertRotation)
            rotationAmount = -rotationAmount;

        // Update target rotation
        targetRotationY += rotationAmount;

        // Apply limits
        if (limitRotation)
            targetRotationY = Mathf.Clamp(targetRotationY, minAngle, maxAngle);

        if (showDebugLogs)
            Debug.Log($"Scrolling: {scrollDelta}, Rotation Amount: {rotationAmount}, Total: {targetRotationY}");

        // Visual feedback
        if (rotationRingEffect != null)
        {
            rotationRingEffect.SetActive(true);
            CancelInvoke(nameof(HideRotationEffect));
            Invoke(nameof(HideRotationEffect), 0.5f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastScrollPosition = eventData.position;
        isScrolling = true;

        if (showDebugLogs)
            Debug.Log("Started dragging for rotation");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isScrolling) return;

        // Calculate drag delta for smooth rotation
        Vector2 dragDelta = eventData.position - lastScrollPosition;

        // FIXED: Natural drag direction (drag right = rotate right)
        float rotationAmount = dragDelta.x * (rotationSpeed * 0.1f);

        // Apply inversion if needed
        if (invertRotation)
            rotationAmount = -rotationAmount;

        targetRotationY += rotationAmount;

        if (limitRotation)
            targetRotationY = Mathf.Clamp(targetRotationY, minAngle, maxAngle);

        lastScrollPosition = eventData.position;

        if (!smoothRotation)
            ApplyRotation(targetRotationY);

        if (showDebugLogs)
            Debug.Log($"Dragging: {dragDelta.x}, Rotation: {targetRotationY}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isScrolling = false;

        if (showDebugLogs)
            Debug.Log("Stopped dragging");
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

    // Public method to reset rotation
    public void ResetRotation()
    {
        targetRotationY = 0f;
        currentRotationY = 0f;
        ApplyRotation(0f);
    }

    // Public method to set custom rotation
    public void SetRotation(float angle)
    {
        targetRotationY = angle;
        currentRotationY = angle;
        ApplyRotation(angle);
    }
}