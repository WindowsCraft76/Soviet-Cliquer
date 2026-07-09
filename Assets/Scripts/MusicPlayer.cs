using UnityEngine;
using UnityEngine.SceneManagement;

public enum MusicContext
{
    Menu,
    Game
}

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private MusicContext Context = MusicContext.Menu;
    [SerializeField] private bool AvoidRepeatingLastTrack = true;

    private AudioSource MusicAudioSource;
    private AudioClip[] tracks;
    private int lastTrackIndex = -1;
    private bool isFocusPaused = false;

    private void Awake()
    {
        MusicAudioSource = GetComponent<AudioSource>();
        if (MusicAudioSource == null)
            MusicAudioSource = gameObject.AddComponent<AudioSource>();

        MusicAudioSource.playOnAwake = false;
        MusicAudioSource.loop = false;
    }

    private void Start()
    {
        string folder = $"Audio/Music/{SceneManager.GetActiveScene().name}";
        tracks = Resources.LoadAll<AudioClip>(folder);

        if (tracks == null || tracks.Length == 0)
        {
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[MusicPlayer] SettingsManager instance not found!");
        }
        else if (Context == MusicContext.Menu)
        {
            MusicAudioSource.volume = SettingsManager.Instance.MenuMusicVolume;
            SettingsManager.Instance.OnMenuMusicVolumeChanged += OnVolumeChanged;
        }
        else
        {
            MusicAudioSource.volume = SettingsManager.Instance.GameMusicVolume;
            SettingsManager.Instance.OnGameMusicVolumeChanged += OnVolumeChanged;
        }

        PlayRandomTrack();
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance == null)
            return;

        if (Context == MusicContext.Menu)
            SettingsManager.Instance.OnMenuMusicVolumeChanged -= OnVolumeChanged;
        else
            SettingsManager.Instance.OnGameMusicVolumeChanged -= OnVolumeChanged;
    }

    private void Update()
    {
        if (isFocusPaused)
            return;

        if (MusicAudioSource == null || tracks == null || tracks.Length == 0)
            return;

        if (!MusicAudioSource.isPlaying)
            PlayRandomTrack();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (MusicAudioSource == null)
            return;

        if (!hasFocus)
        {
            if (MusicAudioSource.isPlaying)
            {
                MusicAudioSource.Pause();
                isFocusPaused = true;
            }
        }
        else if (isFocusPaused)
        {
            MusicAudioSource.UnPause();
            isFocusPaused = false;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        OnApplicationFocus(!pauseStatus);
    }

    private void PlayRandomTrack()
    {
        int index = GetRandomTrackIndex();
        lastTrackIndex = index;

        MusicAudioSource.clip = tracks[index];
        MusicAudioSource.Play();
    }

    private int GetRandomTrackIndex()
    {
        if (tracks.Length == 1)
            return 0;

        int index;
        do
        {
            index = Random.Range(0, tracks.Length);
        }
        while (AvoidRepeatingLastTrack && index == lastTrackIndex);

        return index;
    }

    private void OnVolumeChanged(float volume)
    {
        MusicAudioSource.volume = volume;
    }
}