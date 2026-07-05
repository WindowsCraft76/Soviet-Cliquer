using System;
using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Show player ID")]
    [SerializeField] private TextMeshProUGUI uidText;

    [Tooltip("Show the local version")]
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Copyright")]
    [SerializeField] private TextMeshProUGUI copyrightText;
    [Tooltip("Name displayed")]
    [SerializeField] private string companyName;
    [SerializeField] private int year;

    void Start()
    {
        DisplayUID();
        DisplayVersion();
        DisplayCopyright();
    }

    void OnDisable()
    {
        VersionManager.OnLocalVersionReady -= OnLocalVersionReady;
    }

    public void DisplayUID()
    {
        if (uidText == null)
        {
            Debug.LogError("[MenuUI] TextMeshProUGUI (uidText) not assigned in the inspector.");
            return;
        }

        if (SaveManager.Instance != null)
            uidText.text = SaveManager.Instance.CurrentUserId;
    }

    public void DisplayVersion()
    {
        if (versionText == null)
        {
            Debug.LogError("[MenuUI] TextMeshProUGUI (versionText) not assigned in the inspector.");
            return;
        }

        if (VersionManager.LocalDisplayVersion != null)
        {
            versionText.text = VersionManager.LocalDisplayVersion;
        }
        else
        {
            versionText.text = string.Empty;
            VersionManager.OnLocalVersionReady -= OnLocalVersionReady;
            VersionManager.OnLocalVersionReady += OnLocalVersionReady;
        }
    }

    private void OnLocalVersionReady()
    {
        VersionManager.OnLocalVersionReady -= OnLocalVersionReady;
        if (versionText != null)
            versionText.text = VersionManager.LocalDisplayVersion;
    }

    public void DisplayCopyright()
    {
        if (copyrightText == null)
        {
            Debug.LogError("[MenuUI] TextMeshProUGUI (copyrightText) not assigned in the inspector.");
            return;
        }

        int currentYear = DateTime.Now.Year;
        string yearLabel = currentYear > year
            ? $"{year}-{currentYear}"
            : year.ToString();

        copyrightText.text = $"Copyright © {yearLabel} {companyName}";
    }
}