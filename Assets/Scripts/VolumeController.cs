using UnityEngine;

public class VolumeController : MonoBehaviour
{
    public void ToggleMute()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleMute();

            Debug.Log("[VolumeController] Sound " + (SettingsManager.Instance.mutedSound ? "muted" : "on") + " and saved.");
        }
        else
        {
            Debug.LogError("[VolumeController] SettingsManager not found!");
        }
    }
}