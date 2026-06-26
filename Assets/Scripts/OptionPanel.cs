using UnityEngine;

public class OptionPanel : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Options panel to show / hide.")]
    public GameObject optionPanel;

    void Start()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void ToggleOptions()
    {
        if (optionPanel == null)
        {
            Debug.LogWarning("[OptionPanel] Option panel is not assigned in the inspector.");
            return;
        }

        optionPanel.SetActive(!optionPanel.activeSelf);
    }
}