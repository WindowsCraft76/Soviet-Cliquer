using UnityEngine;
using UnityEngine.UI;

public class AnimationsToggleLinker : MonoBehaviour
{
    [SerializeField] private Toggle AnimationsToggle;

    private void Start()
    {
        if (AnimationsToggle == null)
        {
            Debug.LogError("[AnimationsToggleLinker] AnimationsToggle not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[AnimationsToggleLinker] SettingsManager instance not found!");
            return;
        }

        AnimationsToggle.SetIsOnWithoutNotify(SettingsManager.Instance.AnimationsEnabled);
        AnimationsToggle.onValueChanged.AddListener(SettingsManager.Instance.SetAnimationsEnabled);
    }
}