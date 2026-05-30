using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource; // Primary music source
    public AudioSource secondaryMusicSource; // For crossfade transitions

    [Header("Music Settings")]
    public List<AudioClip> musicTracks = new List<AudioClip>(); // List of background music tracks
    public bool playOnStart = true;
    public bool shuffleMode = false; // Play tracks in random order
    public bool loopTrack = true; // Loop current track
    public bool crossfadeBetweenTracks = true; // Smooth transition between tracks
    public float crossfadeDuration = 2f;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 0.5f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;

    [Header("Fade Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;

    [Header("Track Settings")]
    public int currentTrackIndex = 0;
    public bool autoAdvanceTrack = true;
    public float timeBetweenTracks = 1f;

    [Header("Audio Effects")]
    public bool enableLowPassFilter = false;
    public AudioLowPassFilter lowPassFilter;
    public float lowPassFreqNormal = 22000f;
    public float lowPassFreqLobby = 5000f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showSpectrumVisualization = false;

    private bool isPlaying = false;
    private bool isFading = false;
    private List<int> shuffledPlaylist = new List<int>();
    private int currentShuffleIndex = 0;

    private void Awake()
    {
        // Singleton pattern to persist across scenes
        if (FindObjectsOfType<BackgroundMusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAudioSources();
        CreateShuffledPlaylist();

        if (playOnStart)
        {
            PlayMusic();
        }
    }

    private void InitializeAudioSources()
    {
        // Setup primary audio source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = loopTrack;
        musicSource.volume = 0;
        musicSource.playOnAwake = false;

        // Setup secondary audio source for crossfade
        if (crossfadeBetweenTracks && secondaryMusicSource == null)
        {
            secondaryMusicSource = gameObject.AddComponent<AudioSource>();
            secondaryMusicSource.loop = false;
            secondaryMusicSource.volume = 0;
            secondaryMusicSource.playOnAwake = false;
        }

        // Setup low pass filter if enabled
        if (enableLowPassFilter && lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = lowPassFreqNormal;
        }
    }

    private void CreateShuffledPlaylist()
    {
        if (!shuffleMode || musicTracks.Count == 0) return;

        shuffledPlaylist.Clear();
        for (int i = 0; i < musicTracks.Count; i++)
        {
            shuffledPlaylist.Add(i);
        }

        // Shuffle the playlist
        for (int i = 0; i < shuffledPlaylist.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledPlaylist.Count);
            int temp = shuffledPlaylist[i];
            shuffledPlaylist[i] = shuffledPlaylist[randomIndex];
            shuffledPlaylist[randomIndex] = temp;
        }

        currentShuffleIndex = 0;
        currentTrackIndex = shuffledPlaylist[0];

        if (showDebugLogs)
            Debug.Log($"Created shuffled playlist with {shuffledPlaylist.Count} tracks");
    }

    public void PlayMusic()
    {
        if (musicTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned!");
            return;
        }

        if (isPlaying)
        {
            if (showDebugLogs) Debug.Log("Music already playing");
            return;
        }

        StartCoroutine(PlayMusicCoroutine());
    }

    private IEnumerator PlayMusicCoroutine()
    {
        isPlaying = true;

        // Get current track
        AudioClip track = GetCurrentTrack();
        if (track == null) yield break;

        musicSource.clip = track;
        musicSource.Play();

        // Fade in
        yield return StartCoroutine(FadeVolume(musicSource, 0f, musicVolume * masterVolume, fadeInDuration));

        if (showDebugLogs)
            Debug.Log($"Now playing: {track.name} (Track {currentTrackIndex + 1}/{musicTracks.Count})");

        // Wait for track to end if auto-advance is on
        if (autoAdvanceTrack)
        {
            yield return StartCoroutine(WaitForTrackToEnd());
        }
    }

    private IEnumerator WaitForTrackToEnd()
    {
        while (musicSource.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(timeBetweenTracks);

        // Play next track
        PlayNextTrack();
    }

    public void PlayNextTrack()
    {
        if (!autoAdvanceTrack)
        {
            if (showDebugLogs) Debug.Log("Auto-advance is disabled");
            return;
        }

        if (shuffleMode)
        {
            NextShuffledTrack();
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Count;
        }

        if (crossfadeBetweenTracks)
        {
            StartCoroutine(CrossfadeToNextTrack());
        }
        else
        {
            StopMusic();
            PlayMusic();
        }
    }

    public void PlayPreviousTrack()
    {
        if (shuffleMode)
        {
            PreviousShuffledTrack();
        }
        else
        {
            currentTrackIndex--;
            if (currentTrackIndex < 0)
                currentTrackIndex = musicTracks.Count - 1;
        }

        if (crossfadeBetweenTracks)
        {
            StartCoroutine(CrossfadeToNextTrack());
        }
        else
        {
            StopMusic();
            PlayMusic();
        }
    }

    private void NextShuffledTrack()
    {
        currentShuffleIndex++;
        if (currentShuffleIndex >= shuffledPlaylist.Count)
        {
            // Reshuffle when playlist ends
            CreateShuffledPlaylist();
        }

        currentTrackIndex = shuffledPlaylist[currentShuffleIndex];

        if (showDebugLogs)
            Debug.Log($"Next shuffled track: {musicTracks[currentTrackIndex].name}");
    }

    private void PreviousShuffledTrack()
    {
        currentShuffleIndex--;
        if (currentShuffleIndex < 0)
        {
            currentShuffleIndex = shuffledPlaylist.Count - 1;
        }

        currentTrackIndex = shuffledPlaylist[currentShuffleIndex];

        if (showDebugLogs)
            Debug.Log($"Previous shuffled track: {musicTracks[currentTrackIndex].name}");
    }

    private IEnumerator CrossfadeToNextTrack()
    {
        if (secondaryMusicSource == null)
        {
            Debug.LogWarning("Secondary audio source not available for crossfade");
            yield break;
        }

        AudioClip nextTrack = GetCurrentTrack();
        if (nextTrack == null) yield break;

        // Setup secondary source with next track
        secondaryMusicSource.clip = nextTrack;
        secondaryMusicSource.volume = 0;
        secondaryMusicSource.Play();

        float elapsedTime = 0;

        // Crossfade
        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / crossfadeDuration;

            // Fade out primary, fade in secondary
            musicSource.volume = Mathf.Lerp(musicVolume * masterVolume, 0, t);
            secondaryMusicSource.volume = Mathf.Lerp(0, musicVolume * masterVolume, t);

            yield return null;
        }

        // Swap sources
        AudioSource tempSource = musicSource;
        musicSource = secondaryMusicSource;
        secondaryMusicSource = tempSource;

        // Stop the old source
        secondaryMusicSource.Stop();
        secondaryMusicSource.volume = 0;
        musicSource.volume = musicVolume * masterVolume;

        if (showDebugLogs)
            Debug.Log($"Crossfaded to: {nextTrack.name}");

        // Wait for track to end
        if (autoAdvanceTrack)
        {
            yield return StartCoroutine(WaitForTrackToEnd());
        }
    }

    private IEnumerator FadeVolume(AudioSource source, float fromVolume, float toVolume, float duration)
    {
        if (isFading) yield break;

        isFading = true;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            source.volume = Mathf.Lerp(fromVolume, toVolume, t);
            yield return null;
        }

        source.volume = toVolume;
        isFading = false;
    }

    public void StopMusic()
    {
        StartCoroutine(StopMusicCoroutine());
    }

    private IEnumerator StopMusicCoroutine()
    {
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(musicSource, musicSource.volume, 0, fadeOutDuration));
            musicSource.Stop();
        }

        if (secondaryMusicSource != null && secondaryMusicSource.isPlaying)
        {
            secondaryMusicSource.Stop();
            secondaryMusicSource.volume = 0;
        }

        isPlaying = false;

        if (showDebugLogs)
            Debug.Log("Music stopped");
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            if (showDebugLogs) Debug.Log("Music paused");
        }
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
            if (showDebugLogs) Debug.Log("Music resumed");
        }
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null && !isFading)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        if (showDebugLogs)
            Debug.Log($"Music volume set to: {musicVolume}");
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (musicSource != null && !isFading)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        if (showDebugLogs)
            Debug.Log($"Master volume set to: {masterVolume}");
    }

    public void SetTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < musicTracks.Count)
        {
            currentTrackIndex = trackIndex;
            StopMusic();
            PlayMusic();

            if (showDebugLogs)
                Debug.Log($"Switched to track: {musicTracks[trackIndex].name}");
        }
    }

    public void SetLobbyMode(bool isInLobby)
    {
        if (lowPassFilter != null && enableLowPassFilter)
        {
            float targetFreq = isInLobby ? lowPassFreqLobby : lowPassFreqNormal;
            StartCoroutine(SmoothLowPassTransition(targetFreq));
        }
    }

    private IEnumerator SmoothLowPassTransition(float targetFreq)
    {
        float startFreq = lowPassFilter.cutoffFrequency;
        float elapsed = 0;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            lowPassFilter.cutoffFrequency = Mathf.Lerp(startFreq, targetFreq, t);
            yield return null;
        }

        lowPassFilter.cutoffFrequency = targetFreq;
    }

    private AudioClip GetCurrentTrack()
    {
        if (musicTracks.Count == 0) return null;

        if (currentTrackIndex >= 0 && currentTrackIndex < musicTracks.Count)
        {
            return musicTracks[currentTrackIndex];
        }

        return musicTracks[0];
    }

    public void AddTrack(AudioClip newTrack)
    {
        if (newTrack != null && !musicTracks.Contains(newTrack))
        {
            musicTracks.Add(newTrack);
            CreateShuffledPlaylist();

            if (showDebugLogs)
                Debug.Log($"Added new track: {newTrack.name}");
        }
    }

    public void RemoveTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < musicTracks.Count)
        {
            musicTracks.RemoveAt(trackIndex);
            CreateShuffledPlaylist();

            if (showDebugLogs)
                Debug.Log($"Removed track at index: {trackIndex}");
        }
    }

    private void Update()
    {
        if (showSpectrumVisualization && musicSource.isPlaying)
        {
            float[] spectrum = new float[256];
            musicSource.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);

            // Simple visualization in debug (optional)
            float average = 0;
            for (int i = 0; i < spectrum.Length; i++)
            {
                average += spectrum[i];
            }
            average /= spectrum.Length;

            // Uncomment for debug visualization
            // Debug.Log($"Music Intensity: {average}");
        }
    }

    // Public properties
    public bool IsPlaying { get { return isPlaying; } }
    public float CurrentVolume { get { return musicSource.volume; } }
    public float CurrentTrackProgress
    {
        get
        {
            if (musicSource.clip != null)
                return musicSource.time / musicSource.clip.length;
            return 0;
        }
    }

    public string CurrentTrackName
    {
        get
        {
            if (musicSource.clip != null)
                return musicSource.clip.name;
            return "No Track";
        }
    }
}