using UnityEngine;
using UnityEngine.UI;

public class MuteSoundButtonLinker : MonoBehaviour
{
    [SerializeField] private Button MuteSound;

    private void Start()
    {
        if (MuteSound == null)
        {
            Debug.LogError("[MuteSoundButtonLinker] MuteSound button not assigned!");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[MuteSoundButtonLinker] SettingsManager instance not found!");
            return;
        }

        MuteSound.onClick.AddListener(() => SettingsManager.Instance.ToggleMute());
    }
}