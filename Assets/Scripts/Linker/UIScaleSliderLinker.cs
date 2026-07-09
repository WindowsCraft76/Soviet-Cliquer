using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScaleSliderLinker : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text percentageText;

    private string defaultLabel = "Default";
    private const float DefaultScale = 1f;
    private const float DefaultPercent = 50f;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;

        float percent = ScaleToPercent(SettingsManager.Instance.UIScale);
        slider.SetValueWithoutNotify(percent);
        UpdateText(percent);

        slider.onValueChanged.AddListener(OnSliderChanged);
        SettingsManager.Instance.OnUIScaleChanged += OnUIScaleChanged;
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnUIScaleChanged -= OnUIScaleChanged;
    }

    private void OnSliderChanged(float percent)
    {
        float scale = PercentToScale(percent);
        SettingsManager.Instance.SetUIScale(scale);
        UpdateText(percent);
    }

    private void OnUIScaleChanged(float scale)
    {
        float percent = ScaleToPercent(scale);
        slider.SetValueWithoutNotify(percent);
        UpdateText(percent);
    }

    private void UpdateText(float percent)
    {
        if (percentageText == null)
            return;

        int rounded = Mathf.RoundToInt(percent);
        percentageText.text = Mathf.Approximately(rounded, DefaultPercent) ? "Interface scale: " + defaultLabel : "Interface scale: " + $"{rounded}%";
    }

    private float PercentToScale(float percent)
    {
        float t = percent / 100f;

        if (t <= 0.5f)
            return Mathf.Lerp(SettingsManager.MinUIScale, DefaultScale, t / 0.5f);

        return Mathf.Lerp(DefaultScale, SettingsManager.MaxUIScale, (t - 0.5f) / 0.5f);
    }

    private float ScaleToPercent(float scale)
    {
        if (scale <= DefaultScale)
            return Mathf.InverseLerp(SettingsManager.MinUIScale, DefaultScale, scale) * 50f;

        return 50f + Mathf.InverseLerp(DefaultScale, SettingsManager.MaxUIScale, scale) * 50f;
    }
}