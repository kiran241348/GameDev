using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsPanelManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject settingsPanel; // The settings panel to show/hide
    public Button settingsButton; // Button to open settings
    public Button closeButton; // Cross button to close settings

    [Header("Animation Settings")]
    public bool useAnimation = false; // Enable animations
    public Animator panelAnimator; // Animator for panel animations
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip buttonClickSound;

    [Header("Panel Behavior")]
    public bool startClosed = true; // Panel starts closed
    public bool pauseGameWhenOpen = false; // Pause game when settings open
    public bool blockClicksWhenOpen = true; // Block clicks on other UI when open
    public GameObject blockerPanel; // Optional dark background blocker

    [Header("Settings")]
    public bool enableDebugLogs = true;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        // Initialize panel state
        if (startClosed)
        {
            ClosePanelInstantly();
        }
        else
        {
            OpenPanelInstantly();
        }

        // Setup button listeners
        SetupButtons();

        // Setup audio
        SetupAudio();

        if (enableDebugLogs)
            Debug.Log("Settings Panel Manager initialized. Panel starts closed: " + startClosed);
    }

    private void SetupButtons()
    {
        // Setup settings button
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => {
                PlaySound(buttonClickSound);
                ToggleSettings();
            });
        }
        else
        {
            Debug.LogWarning("Settings button not assigned!");
        }

        // Setup close button (cross button)
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                PlaySound(buttonClickSound);
                CloseSettings();
            });
        }
        else
        {
            Debug.LogWarning("Close button not assigned!");
        }
    }

    private void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ToggleSettings()
    {
        if (isAnimating) return;

        if (isOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    public void OpenSettings()
    {
        if (isOpen || isAnimating) return;

        if (enableDebugLogs)
            Debug.Log("Opening settings panel");

        PlaySound(openSound);

        // Enable panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        // Show blocker if assigned
        if (blockerPanel != null)
        {
            blockerPanel.SetActive(true);
        }

        // Play animation
        if (useAnimation && panelAnimator != null)
        {
            panelAnimator.SetTrigger(openTriggerName);
            StartCoroutine(WaitForAnimation(openTriggerName, true));
        }
        else
        {
            isOpen = true;
        }

        // Pause game if needed
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
            if (enableDebugLogs)
                Debug.Log("Game paused");
        }

        // Block other UI clicks
        if (blockClicksWhenOpen)
        {
            BlockOtherUI(true);
        }
    }

    public void CloseSettings()
    {
        if (!isOpen || isAnimating) return;

        if (enableDebugLogs)
            Debug.Log("Closing settings panel");

        PlaySound(closeSound);

        // Play close animation
        if (useAnimation && panelAnimator != null)
        {
            panelAnimator.SetTrigger(closeTriggerName);
            StartCoroutine(WaitForAnimation(closeTriggerName, false));
        }
        else
        {
            ClosePanelInstantly();
        }

        // Unpause game
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
            if (enableDebugLogs)
                Debug.Log("Game resumed");
        }

        // Unblock other UI clicks
        if (blockClicksWhenOpen)
        {
            BlockOtherUI(false);
        }
    }

    private IEnumerator WaitForAnimation(string triggerName, bool opening)
    {
        isAnimating = true;

        // Wait for animation to complete
        yield return new WaitForEndOfFrame();

        // Get animation length
        AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        yield return new WaitForSeconds(animationLength);

        isAnimating = false;

        if (!opening)
        {
            ClosePanelInstantly();
        }
        else
        {
            isOpen = true;
        }
    }

    private void ClosePanelInstantly()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Hide blocker
        if (blockerPanel != null)
        {
            blockerPanel.SetActive(false);
        }

        isOpen = false;
    }

    private void OpenPanelInstantly()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        if (blockerPanel != null)
        {
            blockerPanel.SetActive(true);
        }

        isOpen = true;
    }

    private void BlockOtherUI(bool block)
    {
        // Find all buttons and make them non-interactable
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            // Don't block settings button and close button
            if (btn == settingsButton || btn == closeButton)
                continue;

            btn.interactable = !block;
        }

        if (enableDebugLogs)
            Debug.Log("Other UI buttons " + (block ? "blocked" : "unblocked"));
    }

    // Public method to close settings from other scripts
    public void ForceCloseSettings()
    {
        if (isOpen)
        {
            CloseSettings();
        }
    }

    // Public method to check if settings is open
    public bool IsSettingsOpen()
    {
        return isOpen;
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (settingsButton != null)
            settingsButton.onClick.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        // Reset time scale if game was paused
        if (pauseGameWhenOpen && Time.timeScale == 0)
        {
            Time.timeScale = 1f;
        }
    }
}