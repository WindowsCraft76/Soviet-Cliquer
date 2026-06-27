using System.IO;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public bool mutedSound = false;
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private static string FilePath
    {
        get
        {
            string roaming = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roaming, ".Soviet-Cliquer", "settings.json");
        }
    }

    [HideInInspector] public bool mutedSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
        Load();
    }

    public void SetMuted(bool muted)
    {
        mutedSound = muted;
        ApplyAudio();
        Save();
    }

    public void ToggleMute()
    {
        SetMuted(!mutedSound);
        Debug.Log($"[VolumeController] Sound {(mutedSound ? "muted" : "enabled")}.");
    }

    public void Save()
    {
        SettingsData data = new SettingsData
        {
            mutedSound = mutedSound
        };

        string dir = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));

        Debug.Log("[SettingsManager] Settings saved.");
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            mutedSound = false;
            ApplyAudio();
            Save();
            return;
        }

        SettingsData data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath));
        mutedSound = data.mutedSound;

        ApplyAudio();

        Debug.Log("[SettingsManager] Settings loaded.");
    }

    public void ApplyAudio()
    {
        AudioListener.volume = mutedSound ? 0f : 1f;
    }
}