using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AdvancedRandomMapLoader : MonoBehaviour
{
    [System.Serializable]
    public class MapData
    {
        public string sceneName;
        public string mapDisplayName;
        public int weight = 1;
        public Sprite previewImage;
        public string description;
        public Color themeColor = Color.white;
    }

    [Header("Maps Configuration")]
    public List<MapData> maps = new List<MapData>();

    [Header("UI References")]
    public Button startButton;
    public Text mapNameText;
    public Image mapPreviewImage;
    public Slider loadingProgressBar;
    public Text loadingPercentageText;
    public GameObject loadingPanel;
    public Text loadingTipText;
    public Image loadingFillImage; // Alternative to slider

    [Header("Loading Settings")]
    public float minLoadTime = 1.5f;
    public bool randomizeMapOnEachPress = true;
    public bool useSmoothLoadingAnimation = true;

    [Header("Loading Tips")]
    [TextArea]
    public string[] loadingTips = new string[]
    {
        "Getting ready for adventure...",
        "Loading amazing worlds...",
        "Almost there!",
        "Preparing surprises...",
        "Gearing up for action!"
    };

    [Header("Animation")]
    public Animator buttonAnimator;
    public string buttonClickTrigger = "Click";
    public Animator loadingPanelAnimator;
    public string showLoadingTrigger = "Show";
    public string hideLoadingTrigger = "Hide";

    private MapData selectedMap;
    private bool isLoading = false;
    private Coroutine loadingCoroutine;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        ShowRandomMapPreview();
    }

    private void ShowRandomMapPreview()
    {
        if (maps.Count == 0) return;

        selectedMap = GetRandomMapWeighted();

        if (mapNameText != null)
        {
            mapNameText.text = selectedMap.mapDisplayName;
            mapNameText.color = selectedMap.themeColor;
        }

        if (mapPreviewImage != null && selectedMap.previewImage != null)
            mapPreviewImage.sprite = selectedMap.previewImage;
    }

    public void OnStartButtonPressed()
    {
        if (isLoading) return;

        if (buttonAnimator != null)
            buttonAnimator.SetTrigger(buttonClickTrigger);

        if (randomizeMapOnEachPress || selectedMap == null)
            selectedMap = GetRandomMapWeighted();

        if (selectedMap == null)
        {
            Debug.LogError("No maps available!");
            return;
        }

        Debug.Log($"Loading map: {selectedMap.mapDisplayName} (Scene: {selectedMap.sceneName})");

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadMapWithProgress(selectedMap.sceneName));
    }

    private IEnumerator LoadMapWithProgress(string sceneName)
    {
        isLoading = true;

        // Show loading panel with animation
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingPanelAnimator != null && !string.IsNullOrEmpty(showLoadingTrigger))
                loadingPanelAnimator.SetTrigger(showLoadingTrigger);
        }

        // Reset progress
        SetLoadingProgress(0f);

        // Set random tip
        if (loadingTipText != null && loadingTips.Length > 0)
            loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;
        float lastProgress = 0f;

        // Loading animation loop
        while (!asyncLoad.isDone)
        {
            float elapsedTime = Time.time - startTime;

            // Get real loading progress (0 to 0.9)
            float realProgress = asyncLoad.progress / 0.9f;

            // Calculate display progress
            float displayProgress;
            if (elapsedTime < minLoadTime)
            {
                // Smoothly interpolate to 100% over minLoadTime
                float t = elapsedTime / minLoadTime;
                if (useSmoothLoadingAnimation)
                    t = Mathf.SmoothStep(0f, 1f, t);
                displayProgress = Mathf.Lerp(0f, 1f, t);

                // Cap at real progress
                displayProgress = Mathf.Min(displayProgress, realProgress);
            }
            else
            {
                displayProgress = realProgress;
            }

            // Update UI smoothly
            if (useSmoothLoadingAnimation)
                displayProgress = Mathf.Lerp(lastProgress, displayProgress, Time.deltaTime * 10f);

            SetLoadingProgress(displayProgress);
            lastProgress = displayProgress;

            // Check if ready to activate
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minLoadTime)
            {
                // Ensure 100% before activating
                SetLoadingProgress(1f);
                yield return new WaitForSeconds(0.2f); // Pause at 100%
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;
    }

    private void SetLoadingProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (loadingProgressBar != null)
            loadingProgressBar.value = progress;

        if (loadingFillImage != null)
            loadingFillImage.fillAmount = progress;

        if (loadingPercentageText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100f);
            loadingPercentageText.text = $"{percentage}%";

            // Color based on percentage
            if (percentage == 100)
                loadingPercentageText.color = Color.green;
            else if (percentage >= 75)
                loadingPercentageText.color = new Color(0.5f, 1f, 0.5f);
            else if (percentage >= 50)
                loadingPercentageText.color = Color.yellow;
            else if (percentage >= 25)
                loadingPercentageText.color = new Color(1f, 0.5f, 0f);
            else
                loadingPercentageText.color = Color.red;
        }
    }

    private MapData GetRandomMapWeighted()
    {
        if (maps.Count == 0) return null;

        // Calculate total weight
        int totalWeight = 0;
        foreach (var map in maps)
            totalWeight += map.weight;

        // Select random based on weight
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var map in maps)
        {
            currentWeight += map.weight;
            if (randomValue < currentWeight)
                return map;
        }

        return maps[0];
    }

    public void RefreshPreview()
    {
        ShowRandomMapPreview();
    }
}