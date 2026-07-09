using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class UIScaleApplier : MonoBehaviour
{
    private CanvasScaler canvasScaler;

    void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
    }

    void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            Apply(SettingsManager.Instance.UIScale);
            SettingsManager.Instance.OnUIScaleChanged += Apply;
        }
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnUIScaleChanged -= Apply;
    }

    private void Apply(float value)
    {
        canvasScaler.scaleFactor = value;
    }
}