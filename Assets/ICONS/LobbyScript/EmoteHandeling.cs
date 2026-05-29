using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyEmoteSystem : MonoBehaviour
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
    public List<EmoteButton> emoteButtons = new List<EmoteButton>(); // Add 8 items

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color playingColor = Color.green;
    public Color cooldownColor = Color.gray;

    [Header("Settings")]
    public float cooldownBetweenEmotes = 0.5f;
    public bool canInterrupt = true;

    [Header("Mouse Settings")]  // ← ADD THIS NEW SECTION
    public bool unlockMouseInLobby = true;
    public KeyCode unlockKey = KeyCode.Escape;

    private string currentEmote = "";
    private bool isOnCooldown = false;
    private Dictionary<string, EmoteButton> emoteDictionary = new Dictionary<string, EmoteButton>();

    private void Start()
    {
        // ← ADD MOUSE UNLOCK CODE HERE
        if (unlockMouseInLobby)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Mouse unlocked for lobby");
        }

        if (lobbyAnimator == null)
            lobbyAnimator = GetComponent<Animator>();

        // Setup dictionary and buttons
        foreach (var emoteBtn in emoteButtons)
        {
            emoteDictionary[emoteBtn.emoteName] = emoteBtn;

            if (emoteBtn.button != null)
            {
                string emoteName = emoteBtn.emoteName; // Capture for closure
                emoteBtn.button.onClick.AddListener(() => PlayEmote(emoteName));

                // Setup visual feedback
                var colors = emoteBtn.button.colors;
                colors.disabledColor = cooldownColor;
                emoteBtn.button.colors = colors;
            }
        }

        ResetAllEmotes();
    }

    // ← ADD THIS UPDATE METHOD
    private void Update()
    {
        // Keep mouse unlocked in lobby
        if (unlockMouseInLobby && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Optional: Press Escape to ensure unlock
        if (Input.GetKeyDown(unlockKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Mouse manually unlocked with Escape key");
        }
    }

    // ← ADD THIS METHOD TO RE-ENABLE WHEN SCRIPT ENABLES
    private void OnEnable()
    {
        if (unlockMouseInLobby)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void PlayEmote(string emoteName)
    {
        // Check if emote exists
        if (!emoteDictionary.ContainsKey(emoteName))
        {
            Debug.LogWarning($"Emote '{emoteName}' not found!");
            return;
        }

        // Check cooldown
        if (isOnCooldown)
        {
            Debug.Log("On cooldown, can't play emote yet!");
            return;
        }

        // Check if already playing this emote
        if (currentEmote == emoteName && !canInterrupt)
            return;

        EmoteButton emoteBtn = emoteDictionary[emoteName];

        // Stop current emote
        if (!string.IsNullOrEmpty(currentEmote))
        {
            StopCurrentEmote();
        }

        // Play new emote
        StartCoroutine(PlayEmoteWithFeedback(emoteName, emoteBtn));
    }

    private IEnumerator PlayEmoteWithFeedback(string emoteName, EmoteButton emoteBtn)
    {
        // Reset all emotes first
        ResetAllEmotes();

        // Set the new emote
        lobbyAnimator.SetBool(emoteName, true);
        currentEmote = emoteName;

        // Visual feedback for playing button
        if (emoteBtn.button != null)
            emoteBtn.button.interactable = false;

        if (emoteBtn.cooldownImage != null)
        {
            emoteBtn.cooldownImage.fillAmount = 1f;
            emoteBtn.cooldownImage.color = playingColor;
        }

        // Show playing visual feedback
        UpdateButtonVisual(emoteBtn, true);

        // Wait for emote duration
        float elapsed = 0;
        while (elapsed < emoteBtn.duration)
        {
            elapsed += Time.deltaTime;

            // Update cooldown image if exists
            if (emoteBtn.cooldownImage != null)
            {
                emoteBtn.cooldownImage.fillAmount = 1 - (elapsed / emoteBtn.duration);
            }

            yield return null;
        }

        // Stop the emote
        StopCurrentEmote();

        // Start cooldown
        StartCoroutine(StartCooldown(emoteBtn));
    }

    private IEnumerator StartCooldown(EmoteButton emoteBtn)
    {
        isOnCooldown = true;

        // Disable all buttons during cooldown
        SetAllButtonsInteractable(false);

        float elapsed = 0;
        while (elapsed < cooldownBetweenEmotes)
        {
            elapsed += Time.deltaTime;

            // Update cooldown visuals for all buttons
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

        // Re-enable all buttons
        SetAllButtonsInteractable(true);

        // Reset cooldown visuals
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

            // Reset visual feedback
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
                // Change button color while playing
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

    // Public method to manually trigger emote from UI button (attach to button onClick)
    public void OnEmoteButtonClick(int emoteNumber)
    {
        string emoteName = $"emote{emoteNumber}";
        PlayEmote(emoteName);
    }

    // Stop all emotes (call this when leaving lobby or starting game)
    public void OnLobbyExit()
    {
        StopAllCoroutines();
        StopCurrentEmote();
        ResetAllEmotes();
        SetAllButtonsInteractable(true);
        isOnCooldown = false;
    }
}