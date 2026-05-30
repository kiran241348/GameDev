using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProfilePanelManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject profilePanel; // The profile panel to show/hide
    public Button profileButton; // Button to open profile (profile icon)
    public Button closeButton; // Cross button to close profile

    [Header("Profile Information (Optional)")]
    public Text playerNameText;
    public Text playerLevelText;
    public Text playerXPText;
    public Image playerAvatarImage;
    public Slider xpProgressSlider;

    [Header("Animation Settings")]
    public bool useAnimation = false;
    public Animator panelAnimator;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip buttonClickSound;

    [Header("Panel Behavior")]
    public bool startClosed = true;
    public bool closeOnBackgroundClick = true; // Close when clicking outside
    public GameObject backgroundBlocker; // Optional dark background blocker

    [Header("Player Data (Example)")]
    public string playerName = "Player Name";
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpRequiredForNextLevel = 100;

    [Header("Debug")]
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

        // Setup buttons
        SetupButtons();

        // Setup audio
        SetupAudio();

        // Update profile information
        UpdateProfileInfo();

        if (enableDebugLogs)
            Debug.Log("Profile Panel Manager initialized. Panel starts closed: " + startClosed);
    }

    private void SetupButtons()
    {
        // Setup profile button
        if (profileButton != null)
        {
            profileButton.onClick.RemoveAllListeners();
            profileButton.onClick.AddListener(() => {
                PlaySound(buttonClickSound);
                OpenProfile();
            });
        }
        else
        {
            Debug.LogWarning("Profile button not assigned!");
        }

        // Setup close button (cross button)
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                PlaySound(buttonClickSound);
                CloseProfile();
            });
        }
        else
        {
            Debug.LogWarning("Close button not assigned!");
        }

        // Setup background click to close
        if (closeOnBackgroundClick && backgroundBlocker != null)
        {
            Button blockerButton = backgroundBlocker.GetComponent<Button>();
            if (blockerButton == null)
                blockerButton = backgroundBlocker.AddComponent<Button>();

            blockerButton.onClick.RemoveAllListeners();
            blockerButton.onClick.AddListener(() => {
                if (isOpen)
                {
                    PlaySound(buttonClickSound);
                    CloseProfile();
                }
            });
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

    public void OpenProfile()
    {
        if (isOpen || isAnimating) return;

        if (enableDebugLogs)
            Debug.Log("Opening profile panel");

        PlaySound(openSound);

        // Update profile info before showing
        UpdateProfileInfo();

        // Enable panel
        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
        }

        // Show blocker if assigned
        if (backgroundBlocker != null)
        {
            backgroundBlocker.SetActive(true);
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
    }

    public void CloseProfile()
    {
        if (!isOpen || isAnimating) return;

        if (enableDebugLogs)
            Debug.Log("Closing profile panel");

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
    }

    private IEnumerator WaitForAnimation(string triggerName, bool opening)
    {
        isAnimating = true;

        // Wait for animation to complete
        yield return new WaitForEndOfFrame();

        if (panelAnimator != null)
        {
            AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
            float animationLength = stateInfo.length;
            yield return new WaitForSeconds(animationLength);
        }

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
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }

        // Hide blocker
        if (backgroundBlocker != null)
        {
            backgroundBlocker.SetActive(false);
        }

        isOpen = false;
    }

    private void OpenPanelInstantly()
    {
        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
        }

        if (backgroundBlocker != null)
        {
            backgroundBlocker.SetActive(true);
        }

        isOpen = true;
    }

    // Public method to update profile information
    public void UpdateProfileInfo()
    {
        if (playerNameText != null)
            playerNameText.text = playerName;

        if (playerLevelText != null)
            playerLevelText.text = "Level " + playerLevel.ToString();

        if (playerXPText != null)
            playerXPText.text = $"XP: {currentXP}/{xpRequiredForNextLevel}";

        if (xpProgressSlider != null)
            xpProgressSlider.value = (float)currentXP / xpRequiredForNextLevel;

        if (enableDebugLogs)
            Debug.Log("Profile information updated");
    }

    // Public methods to update player data
    public void SetPlayerName(string name)
    {
        playerName = name;
        UpdateProfileInfo();
    }

    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
        UpdateProfileInfo();
    }

    public void SetPlayerXP(int xp)
    {
        currentXP = xp;
        UpdateProfileInfo();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        // Check for level up
        while (currentXP >= xpRequiredForNextLevel)
        {
            currentXP -= xpRequiredForNextLevel;
            playerLevel++;

            if (enableDebugLogs)
                Debug.Log($"Level Up! New Level: {playerLevel}");
        }

        UpdateProfileInfo();
    }

    // Public method to set avatar
    public void SetPlayerAvatar(Sprite avatar)
    {
        if (playerAvatarImage != null)
            playerAvatarImage.sprite = avatar;
    }

    // Public method to force close from other scripts
    public void ForceCloseProfile()
    {
        if (isOpen)
        {
            CloseProfile();
        }
    }

    // Public method to check if profile is open
    public bool IsProfileOpen()
    {
        return isOpen;
    }

    // Public method to toggle profile
    public void ToggleProfile()
    {
        if (isOpen)
            CloseProfile();
        else
            OpenProfile();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (profileButton != null)
            profileButton.onClick.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }
}