using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResetDataButtonLinker : MonoBehaviour
{
    [SerializeField] private Button ResetData;
    [SerializeField] private TMP_Text ResetDataTextTMP;

    [Header("Config")]
    [SerializeField] private string defaultLabel = "Reset";
    [SerializeField] private string confirmLabel = "You sure?";
    [SerializeField] private float confirmTimeout = 10f;

    private bool awaitingConfirmation = false;
    private float confirmTimer = 0f;

    private void Start()
    {
        if (ResetData == null)
        {
            Debug.LogError("[ResetButtonLinker] ResetData button not assigned!");
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[ResetButtonLinker] SaveManager instance not found!");
            return;
        }

        SetLabel(defaultLabel);
        ResetData.onClick.AddListener(OnResetClicked);
    }

    private void Update()
    {
        if (!awaitingConfirmation)
            return;

        confirmTimer -= Time.deltaTime;
        if (confirmTimer <= 0f)
        {
            CancelConfirmation();
        }
    }

    private void OnResetClicked()
    {
        if (!awaitingConfirmation)
        {
            awaitingConfirmation = true;
            confirmTimer = confirmTimeout;
            SetLabel(confirmLabel);
            return;
        }

        SaveManager.Instance.Reset();
        CancelConfirmation();
    }

    private void CancelConfirmation()
    {
        awaitingConfirmation = false;
        confirmTimer = 0f;
        SetLabel(defaultLabel);
    }

    private void SetLabel(string label)
    {
        if (ResetDataTextTMP != null)
        {
            ResetDataTextTMP.text = label;
        }
        else
        {
            Debug.LogWarning("[ResetDataButtonLinker] No text field assigned.");
        }
    }
}