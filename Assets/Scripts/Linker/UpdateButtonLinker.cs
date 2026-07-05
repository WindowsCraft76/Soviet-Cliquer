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
        }

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

        // Subscribed in Awake (not OnEnable) on purpose: SetUpdateButtonVisible(false)
        // below can deactivate this component's own GameObject (when updateButtonObject
        // is the object the script lives on) before OnEnable ever runs. Awake still runs
        // fully before that happens, so this is the only place guaranteed to fire.
        VersionManager.OnUpdateAvailable += HandleUpdateAvailable;

        SetUpdateButtonVisible(false);

        // The check may have already completed (e.g. in a scene loaded before this
        // one) before this button existed to hear the event — catch up here.
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