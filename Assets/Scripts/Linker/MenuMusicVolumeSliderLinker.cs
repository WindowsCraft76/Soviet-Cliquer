using UnityEngine;
using UnityEngine.UI;

public class MenuMusicVolumeSliderLinker : MonoBehaviour
{
    [SerializeField] private Slider MenuMusicVolumeSlider;

    private void Start()
    {
        if (MenuMusicVolumeSlider == null)
        {
            Debug.LogError("[MenuMusicVolumeSliderLinker] MenuMusicVolumeSlider not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[MenuMusicVolumeSliderLinker] SettingsManager instance not found!");
            return;
        }

        MenuMusicVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.MenuMusicVolume);
        MenuMusicVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMenuMusicVolume);
    }
}