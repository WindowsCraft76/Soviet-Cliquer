using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateButtonLinker : MonoBehaviour
{
    [SerializeField] private Button button;
    [Tooltip("Button that appears when an update is available")]
    [SerializeField] private GameObject updateButtonObject;

    [Tooltip("Update button text (optional)")]
    [SerializeField] private TMP_Text updateButtonLabel;

    [Header("Configuration")]

    [Tooltip("Download URL")]
    [SerializeField] private string downloadUrl;

    private void Awake()
    {
        if (button == null && updateButtonObject != null)
        {
            button = updateButtonObject.GetComponent<Button>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }//

        if (button == null)
        {
            Debug.LogError($"[UpdateButtonLinker] No Button assigned or found on {gameObject.name}");
        }
        else
        {
            button.onClick.AddListener(HandleClick);
        }

        if (updateButtonLabel == null && updateButtonObject != null)
        {
            updateButtonLabel = updateButtonObject.GetComponentInChildren<TMP_Text>(true);
        }

        VersionManager.OnUpdateAvailable += HandleUpdateAvailable;

        SetUpdateButtonVisible(false);

        if (VersionManager.HasCheckedForUpdate && VersionManager.IsUpdateAvailable)
        {
            HandleUpdateAvailable(VersionManager.IsFirstInstall);
        }
    }

    private void OnDestroy()
    {
        VersionManager.OnUpdateAvailable -= HandleUpdateAvailable;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleUpdateAvailable(bool isFirstInstall)
    {
        SetUpdateButtonVisible(true);

        if (updateButtonLabel != null)
        {
            updateButtonLabel.text = isFirstInstall
                ? "Download latest version!"
                : "New update available!";
        }
    }

    private void SetUpdateButtonVisible(bool visible)
    {
        if (updateButtonObject != null)
        {
            updateButtonObject.SetActive(visible);
        }
    }

    private void HandleClick()
    {
        if (!string.IsNullOrEmpty(downloadUrl))
        {
            Application.OpenURL(downloadUrl);
            Debug.Log($"[UpdateButtonLinker] Opening: {downloadUrl}");
        }
    }
}