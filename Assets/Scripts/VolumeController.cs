using UnityEngine;

public class VolumeController : MonoBehaviour
{
    public void ToggleMute()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.isMuted = !SaveManager.Instance.isMuted;

            SaveManager.Instance.ApplyAudioSettings();

            SaveManager.Instance.Save();
            
            Debug.Log("[VolumeController] Sound " + (SaveManager.Instance.isMuted ? "muted" : "on") + " and saved.");
        }
    }
}