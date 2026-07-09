using UnityEngine;
using UnityEngine.UI;

public class GameMusicVolumeSliderLinker : MonoBehaviour
{
    [SerializeField] private Slider GameMusicVolumeSlider;

    private void Start()
    {
        if (GameMusicVolumeSlider == null)
        {
            Debug.LogError("[GameMusicVolumeSliderLinker] GameMusicVolumeSlider not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[GameMusicVolumeSliderLinker] SettingsManager instance not found!");
            return;
        }

        GameMusicVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.GameMusicVolume);
        GameMusicVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetGameMusicVolume);
    }
}