using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public float musicVolume = 1f;
    public float clickVolume = 1f;
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

    public float MusicVolume { get; private set; } = 1f;
    public float ClickVolume { get; private set; } = 1f;

    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnClickVolumeChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
        Load();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged?.Invoke(MusicVolume);
        Save();
    }

    public void SetClickVolume(float volume)
    {
        ClickVolume = Mathf.Clamp01(volume);
        OnClickVolumeChanged?.Invoke(ClickVolume);
        Save();
    }

    public void Save()
    {
        SettingsData data = new SettingsData
        {
            musicVolume = MusicVolume,
            clickVolume = ClickVolume
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
            MusicVolume = 1f;
            ClickVolume = 1f;
            Save();
            return;
        }

        SettingsData data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath));
        MusicVolume = data.musicVolume;
        ClickVolume = data.clickVolume;

        OnMusicVolumeChanged?.Invoke(MusicVolume);
        OnClickVolumeChanged?.Invoke(ClickVolume);

        Debug.Log("[SettingsManager] Settings loaded.");
    }
}