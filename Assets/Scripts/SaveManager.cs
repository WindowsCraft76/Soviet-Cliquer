using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class SaveData
{
    public int counter = 0;
    public string saveDate = "";
    public bool isMuted = false;
    public string userId = "";
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private static string FilePath
    {
        get
        {
            string roamingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roamingPath, ".Soviet-Cliquer", "data.json");
        }
    }

    [HideInInspector]
    public int currentCounter = 0;
    
    [HideInInspector]
    public bool isMuted = false;

    [HideInInspector]
    public string currentUserId = "";

    private float saveTimer = 0f;
    private const float SAVE_INTERVAL = 60f;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        saveTimer += Time.deltaTime;
        if (saveTimer >= SAVE_INTERVAL)
        {
            saveTimer = 0f;
            Save();
            Debug.Log("[SaveManager] Auto-save.");
        }
    }

    void OnApplicationQuit()
    {
        Save();
        Debug.Log("[SaveManager] Auto-save.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Save();
    }

    public void Save()
    {
        if (!IsValidUUID(currentUserId))
        {
            currentUserId = GenerateNewUUID();
            Debug.LogWarning("[SaveManager] Invalid UUID before save. Generating a new one.");
        }

        SaveData data = new SaveData
        {
            counter = currentCounter,
            isMuted = isMuted,
            userId = currentUserId,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")
        };

        string directory = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(FilePath, json);
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("[SaveManager] No save file found. Initializing data.");
            currentCounter = 0;
            isMuted = false;
            currentUserId = GenerateNewUUID();
            ApplyAudioSettings();
            Save();
            return;
        }

        string json = File.ReadAllText(FilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        currentCounter = data.counter;
        isMuted = data.isMuted;

        if (IsValidUUID(data.userId))
        {
            currentUserId = data.userId;
        }
        else
        {
            Debug.LogError("[SaveManager] Corrupted or missing UUID detected in data.json! Generating a new one...");
            currentUserId = GenerateNewUUID();
            Save();
        }

        ApplyAudioSettings();
        Debug.Log("[SaveManager] Save data loaded successfully.");
    }

    public void Reset()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        currentCounter = 0;
        isMuted = false;
        currentUserId = GenerateNewUUID();
        ApplyAudioSettings();

        Load();
        Debug.Log("[SaveManager] Reset save data.");
    }

    public void ApplyAudioSettings()
    {
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    private string GenerateNewUUID()
    {
        return System.Guid.NewGuid().ToString();
    }

    private bool IsValidUUID(string uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return false;
        
        return System.Guid.TryParse(uuid, out _);
    }
}