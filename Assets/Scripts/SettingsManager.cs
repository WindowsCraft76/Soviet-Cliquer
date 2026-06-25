using UnityEngine;
using System.IO;

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
            string roamingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roamingPath, ".Soviet-Cliquer", "settings.json");
        }
    }

    [HideInInspector]
    public bool mutedSound = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Load();
    }

    public void Save()
    {
        SettingsData data = new SettingsData
        {
            mutedSound = mutedSound
        };

        string directory = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(FilePath, json);
        Debug.Log("[SettingsManager] Settings saved.");
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("[SettingsManager] No settings file found. Using defaults.");
            mutedSound = false;
            ApplyAudioSettings();
            Save();
            return;
        }

        string json = File.ReadAllText(FilePath);
        SettingsData data = JsonUtility.FromJson<SettingsData>(json);

        mutedSound = data.mutedSound;
        ApplyAudioSettings();
        Debug.Log("[SettingsManager] Settings loaded.");
    }

    public void SetMuted(bool muted)
    {
        mutedSound = muted;
        ApplyAudioSettings();
        Save();
    }

    public void ToggleMute()
    {
        SetMuted(!mutedSound);
    }

    public void ApplyAudioSettings()
    {
        AudioListener.volume = mutedSound ? 0f : 1f;
    }
}