using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    [Header("UI Configuration")]
    [Tooltip("Drag the options panel you want to show/hide here.")]
    public GameObject optionsPanel;

    void Start()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void ToggleOptions()
    {
        if (optionsPanel != null)
        {
            bool isActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("The options panel has not been assigned in the inspector!");
        }
    }
}