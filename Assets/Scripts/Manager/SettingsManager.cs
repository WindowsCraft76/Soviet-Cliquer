using System;
using System.IO;
using UnityEngine;

public enum NumberFormatType
{
    Full,
    Separated,
    Abbreviated
}

[System.Serializable]
public class SettingsData
{
    public float menuMusicVolume = 1f;
    public float clickVolume = 1f;
    public float uiScale = 1f;
    public float gameMusicVolume = 1f;
    public NumberFormatType numberFormat = NumberFormatType.Abbreviated;
    public bool animationsEnabled = true;
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

    public const float MinUIScale = 0.75f;
    public const float MaxUIScale = 1.5f;

    public float MenuMusicVolume { get; private set; } = 1f;
    public float ClickVolume { get; private set; } = 1f;
    public float UIScale { get; private set; } = 1f;
    public float GameMusicVolume { get; private set; } = 1f;
    public NumberFormatType NumberFormat { get; private set; } = NumberFormatType.Abbreviated;
    public bool AnimationsEnabled { get; private set; } = true;

    public event Action<float> OnMenuMusicVolumeChanged;
    public event Action<float> OnClickVolumeChanged;
    public event Action<float> OnUIScaleChanged;
    public event Action<float> OnGameMusicVolumeChanged;
    public event Action<NumberFormatType> OnNumberFormatChanged;
    public event Action<bool> OnAnimationsEnabledChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void SetMenuMusicVolume(float volume)
    {
        MenuMusicVolume = Mathf.Clamp01(volume);
        OnMenuMusicVolumeChanged?.Invoke(MenuMusicVolume);
        Save();
    }

    public void SetClickVolume(float volume)
    {
        ClickVolume = Mathf.Clamp01(volume);
        OnClickVolumeChanged?.Invoke(ClickVolume);
        Save();
    }

    public void SetUIScale(float scale)
    {
        UIScale = Mathf.Clamp(scale, MinUIScale, MaxUIScale);
        OnUIScaleChanged?.Invoke(UIScale);
        Save();
    }

    public void SetGameMusicVolume(float volume)
    {
        GameMusicVolume = Mathf.Clamp01(volume);
        OnGameMusicVolumeChanged?.Invoke(GameMusicVolume);
        Save();
    }

    public void SetNumberFormat(NumberFormatType format)
    {
        NumberFormat = format;
        OnNumberFormatChanged?.Invoke(NumberFormat);
        Save();
    }

    public void SetAnimationsEnabled(bool enabled)
    {
        AnimationsEnabled = enabled;
        OnAnimationsEnabledChanged?.Invoke(AnimationsEnabled);
        Save();
    }

    public void Save()
    {
        SettingsData data = new SettingsData
        {
            menuMusicVolume = MenuMusicVolume,
            clickVolume = ClickVolume,
            uiScale = UIScale,
            gameMusicVolume = GameMusicVolume,
            numberFormat = NumberFormat,
            animationsEnabled = AnimationsEnabled
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
            MenuMusicVolume = 1f;
            ClickVolume = 1f;
            UIScale = 1f;
            GameMusicVolume = 1f;
            NumberFormat = NumberFormatType.Abbreviated;
            AnimationsEnabled = true;
            Save();
            return;
        }

        SettingsData data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath));
        MenuMusicVolume = data.menuMusicVolume;
        ClickVolume = data.clickVolume;
        UIScale = data.uiScale > 0f ? data.uiScale : 1f;
        GameMusicVolume = data.gameMusicVolume;
        NumberFormat = data.numberFormat;
        AnimationsEnabled = data.animationsEnabled;

        OnMenuMusicVolumeChanged?.Invoke(MenuMusicVolume);
        OnClickVolumeChanged?.Invoke(ClickVolume);
        OnUIScaleChanged?.Invoke(UIScale);
        OnGameMusicVolumeChanged?.Invoke(GameMusicVolume);
        OnNumberFormatChanged?.Invoke(NumberFormat);
        OnAnimationsEnabledChanged?.Invoke(AnimationsEnabled);

        Debug.Log("[SettingsManager] Settings loaded.");
    }
}