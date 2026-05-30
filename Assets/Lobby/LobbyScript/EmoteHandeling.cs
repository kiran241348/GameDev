using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LobbyEmoteSystem : MonoBehaviour, IPointerClickHandler
{
    [System.Serializable]
    public class EmoteButton
    {
        public string emoteName; // emote1, emote2, etc.
        public Button button;
        public Image cooldownImage; // Optional cooldown overlay
        public float duration = 2f;
    }

    [Header("Animator")]
    public Animator lobbyAnimator;

    [Header("Emote Buttons")]
    public List<EmoteButton> emoteButtons = new List<EmoteButton>();

    [Header("Emote UI Panel")]
    public GameObject emoteUIPanel; // The panel containing all emote buttons
    public bool hideOnEmoteClick = true; // Hide panel when emote button is clicked

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color playingColor = Color.green;
    public Color cooldownColor = Color.gray;

    [Header("Settings")]
    public float cooldownBetweenEmotes = 0.5f;
    public bool canInterrupt = true;

    [Header("Mouse Settings")]
    public bool unlockMouseInLobby = true;
    public KeyCode unlockKey = KeyCode.Escape;

    [Header("Character Tap Settings")]
    public Transform characterTransform; // The character to tap on
    public float tapDetectionRadius = 1.5f;
    public LayerMask characterLayer = -1;
    public float tapDurationThreshold = 0.2f; // Minimum tap duration
    public float maxDragDistance = 10f; // Max drag distance to still count as tap

    // Rotation state tracking
    private bool isRotating = false;
    private string currentEmote = "";
    private bool isOnCooldown = false;
    private Dictionary<string, EmoteButton> emoteDictionary = new Dictionary<string, EmoteButton>();

    // Tap detection variables
    private bool isTapping = false;
    private float tapStartTime = 0f;
    private Vector2 tapStartPosition;

    private void Start()
    {
        // Unlock mouse for lobby
        if (unlockMouseInLobby)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Mouse unlocked for lobby");
        }

        if (lobbyAnimator == null)
            lobbyAnimator = GetComponent<Animator>();

        if (characterTransform == null)
            characterTransform = transform;

        // Setup dictionary and buttons
        foreach (var emoteBtn in emoteButtons)
        {
            emoteDictionary[emoteBtn.emoteName] = emoteBtn;

            if (emoteBtn.button != null)
            {
                string emoteName = emoteBtn.emoteName;
                emoteBtn.button.onClick.AddListener(() => OnEmoteButtonClicked(emoteName));

                var colors = emoteBtn.button.colors;
                colors.disabledColor = cooldownColor;
                emoteBtn.button.colors = colors;
            }
        }

        // Hide emote UI by default
        if (emoteUIPanel != null)
        {
            emoteUIPanel.SetActive(false);
            Debug.Log("Emote UI hidden by default");
        }

        ResetAllEmotes();
    }

    private void Update()
    {
        // Keep mouse unlocked in lobby
        if (unlockMouseInLobby && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKeyDown(unlockKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Handle character tap
        HandleCharacterTap();
    }

    // Public method to be called from rotation script
    public void SetRotating(bool rotating)
    {
        isRotating = rotating;

        // If rotating and emote panel is open, close it
        if (isRotating && emoteUIPanel != null && emoteUIPanel.activeSelf)
        {
            HideEmoteUI();
        }

        Debug.Log($"Emote System - Rotating state changed to: {rotating}");
    }

    private void HandleCharacterTap()
    {
        // For PC/Mouse
        if (Input.GetMouseButtonDown(0))
        {
            OnTapStart(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isTapping)
        {
            OnTapEnd(Input.mousePosition);
        }

        // For Mobile Touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnTapStart(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended && isTapping)
            {
                OnTapEnd(touch.position);
            }
        }
    }

    private void OnTapStart(Vector2 position)
    {
        // Don't start tap if currently rotating
        if (isRotating) return;

        isTapping = true;
        tapStartTime = Time.time;
        tapStartPosition = position;
    }

    private void OnTapEnd(Vector2 endPosition)
    {
        if (!isTapping) return;

        // Calculate tap duration and drag distance
        float tapDuration = Time.time - tapStartTime;
        float dragDistance = Vector2.Distance(tapStartPosition, endPosition);

        // Check if this is a valid tap (not a drag and not too long)
        bool isValidTap = tapDuration <= tapDurationThreshold && dragDistance <= maxDragDistance;

        if (isValidTap && !isRotating)
        {
            // Check if tap is on character
            if (IsTapOnCharacter(endPosition))
            {
                Debug.Log("Intentional tap detected on character!");
                ToggleEmoteUI();
            }
        }

        isTapping = false;
    }

    private bool IsTapOnCharacter(Vector2 screenPosition)
    {
        // Raycast to check if tap is on character
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, characterLayer))
            {
                if (hit.transform == characterTransform || hit.transform.IsChildOf(characterTransform))
                {
                    return true;
                }
            }

            // Alternative: Check distance from character on screen
            if (characterTransform != null)
            {
                Vector3 characterScreenPos = Camera.main.WorldToScreenPoint(characterTransform.position);
                float distance = Vector2.Distance(screenPosition, characterScreenPos);

                float screenRadius = tapDetectionRadius * 50f;
                if (distance < screenRadius)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ShowEmoteUI()
    {
        // Don't show if rotating
        if (isRotating)
        {
            Debug.Log("Can't show emote UI while rotating");
            return;
        }

        if (emoteUIPanel != null && !emoteUIPanel.activeSelf)
        {
            emoteUIPanel.SetActive(true);
            Debug.Log("Emote UI shown");
        }
    }

    public void HideEmoteUI()
    {
        if (emoteUIPanel != null && emoteUIPanel.activeSelf)
        {
            emoteUIPanel.SetActive(false);
            Debug.Log("Emote UI hidden");
        }
    }

    public void ToggleEmoteUI()
    {
        if (emoteUIPanel != null)
        {
            if (emoteUIPanel.activeSelf)
            {
                HideEmoteUI();
            }
            else
            {
                ShowEmoteUI();
            }
        }
    }

    private void OnEmoteButtonClicked(string emoteName)
    {
        PlayEmote(emoteName);

        if (hideOnEmoteClick)
        {
            HideEmoteUI();
        }
    }

    public void PlayEmote(string emoteName)
    {
        if (!emoteDictionary.ContainsKey(emoteName))
        {
            Debug.LogWarning($"Emote '{emoteName}' not found!");
            return;
        }

        if (isOnCooldown)
        {
            Debug.Log("On cooldown, can't play emote yet!");
            return;
        }

        if (currentEmote == emoteName && !canInterrupt)
            return;

        EmoteButton emoteBtn = emoteDictionary[emoteName];

        if (!string.IsNullOrEmpty(currentEmote))
        {
            StopCurrentEmote();
        }

        StartCoroutine(PlayEmoteWithFeedback(emoteName, emoteBtn));
    }

    private IEnumerator PlayEmoteWithFeedback(string emoteName, EmoteButton emoteBtn)
    {
        ResetAllEmotes();

        lobbyAnimator.SetBool(emoteName, true);
        currentEmote = emoteName;

        if (emoteBtn.button != null)
            emoteBtn.button.interactable = false;

        if (emoteBtn.cooldownImage != null)
        {
            emoteBtn.cooldownImage.fillAmount = 1f;
            emoteBtn.cooldownImage.color = playingColor;
        }

        UpdateButtonVisual(emoteBtn, true);

        float elapsed = 0;
        while (elapsed < emoteBtn.duration)
        {
            elapsed += Time.deltaTime;

            if (emoteBtn.cooldownImage != null)
            {
                emoteBtn.cooldownImage.fillAmount = 1 - (elapsed / emoteBtn.duration);
            }

            yield return null;
        }

        StopCurrentEmote();
        StartCoroutine(StartCooldown(emoteBtn));
    }

    private IEnumerator StartCooldown(EmoteButton emoteBtn)
    {
        isOnCooldown = true;

        SetAllButtonsInteractable(false);

        float elapsed = 0;
        while (elapsed < cooldownBetweenEmotes)
        {
            elapsed += Time.deltaTime;

            foreach (var btn in emoteButtons)
            {
                if (btn.cooldownImage != null)
                {
                    btn.cooldownImage.fillAmount = 1 - (elapsed / cooldownBetweenEmotes);
                }
            }

            yield return null;
        }

        isOnCooldown = false;
        SetAllButtonsInteractable(true);

        foreach (var btn in emoteButtons)
        {
            if (btn.cooldownImage != null)
            {
                btn.cooldownImage.fillAmount = 0;
                btn.cooldownImage.color = normalColor;
            }
        }
    }

    private void StopCurrentEmote()
    {
        if (!string.IsNullOrEmpty(currentEmote))
        {
            lobbyAnimator.SetBool(currentEmote, false);

            if (emoteDictionary.ContainsKey(currentEmote))
            {
                UpdateButtonVisual(emoteDictionary[currentEmote], false);
            }

            currentEmote = "";
        }
    }

    private void ResetAllEmotes()
    {
        foreach (var emoteParam in emoteDictionary.Keys)
        {
            lobbyAnimator.SetBool(emoteParam, false);
        }
    }

    private void UpdateButtonVisual(EmoteButton emoteBtn, bool isPlaying)
    {
        if (emoteBtn.button != null)
        {
            var colors = emoteBtn.button.colors;

            if (isPlaying)
            {
                colors.normalColor = playingColor;
                colors.selectedColor = playingColor;
            }
            else
            {
                colors.normalColor = normalColor;
                colors.selectedColor = normalColor;
            }

            emoteBtn.button.colors = colors;
        }
    }

    private void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var emoteBtn in emoteButtons)
        {
            if (emoteBtn.button != null)
            {
                emoteBtn.button.interactable = interactable;
            }
        }
    }

    public void OnEmoteButtonClick(int emoteNumber)
    {
        string emoteName = $"emote{emoteNumber}";
        PlayEmote(emoteName);

        if (hideOnEmoteClick)
        {
            HideEmoteUI();
        }
    }

    public void OnLobbyExit()
    {
        StopAllCoroutines();
        StopCurrentEmote();
        ResetAllEmotes();
        SetAllButtonsInteractable(true);
        isOnCooldown = false;
        HideEmoteUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRotating)
        {
            ToggleEmoteUI();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (characterTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(characterTransform.position, tapDetectionRadius);
        }
    }
}