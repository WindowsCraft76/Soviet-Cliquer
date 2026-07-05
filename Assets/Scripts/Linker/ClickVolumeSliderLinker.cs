using UnityEngine;
using UnityEngine.UI;

public class ClickVolumeSliderLinker : MonoBehaviour
{
    [SerializeField] private Slider ClickVolumeSlider;

    private void Start()
    {
        if (ClickVolumeSlider == null)
        {
            Debug.LogError("[ClickVolumeSliderLinker] ClickVolumeSlider not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[ClickVolumeSliderLinker] SettingsManager instance not found!");
            return;
        }

        ClickVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.ClickVolume);
        ClickVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetClickVolume);
    }
}