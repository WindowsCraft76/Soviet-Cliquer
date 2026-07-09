using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberFormatDropdownLinker : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown NumberFormatDropdown;

    private static readonly NumberFormatType[] Formats =
    {
        NumberFormatType.Full,
        NumberFormatType.Separated,
        NumberFormatType.Abbreviated
    };

    private static readonly string[] Labels =
    {
        "1234567",
        "1.234.567",
        "1.23 M"
    };

    private void Start()
    {
        if (NumberFormatDropdown == null)
        {
            Debug.LogError("[NumberFormatDropdownLinker] NumberFormatDropdown not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[NumberFormatDropdownLinker] SettingsManager instance not found!");
            return;
        }

        NumberFormatDropdown.ClearOptions();
        NumberFormatDropdown.AddOptions(new List<string>(Labels));

        int currentIndex = System.Array.IndexOf(Formats, SettingsManager.Instance.NumberFormat);
        NumberFormatDropdown.SetValueWithoutNotify(currentIndex >= 0 ? currentIndex : 0);

        NumberFormatDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDestroy()
    {
        if (NumberFormatDropdown != null)
            NumberFormatDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetNumberFormat(Formats[index]);
    }
}