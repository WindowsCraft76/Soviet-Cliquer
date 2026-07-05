using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[MusicController] SettingsManager instance not found!");
            return;
        }

        ApplyVolume(SettingsManager.Instance.MusicVolume);
        SettingsManager.Instance.OnMusicVolumeChanged += ApplyVolume;
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnMusicVolumeChanged -= ApplyVolume;
    }

    private void ApplyVolume(float volume)
    {
        _audioSource.volume = volume;
    }
}