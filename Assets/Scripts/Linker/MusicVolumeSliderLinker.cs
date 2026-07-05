using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSliderLinker : MonoBehaviour
{
    [SerializeField] private Slider MusicVolumeSlider;

    private void Start()
    {
        if (MusicVolumeSlider == null)
        {
            Debug.LogError("[MusicVolumeSliderLinker] MusicVolumeSlider not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[MusicVolumeSliderLinker] SettingsManager instance not found!");
            return;
        }

        MusicVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
        MusicVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);
    }
}