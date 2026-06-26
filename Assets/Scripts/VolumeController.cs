using UnityEngine;

public class VolumeController : MonoBehaviour
{
    public void ToggleMute()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("[VolumeController] SettingsManager not found.");
            return;
        }

        SettingsManager.Instance.ToggleMute();
        Debug.Log($"[VolumeController] Sound {(SettingsManager.Instance.mutedSound ? "muted" : "enabled")}.");
    }
}