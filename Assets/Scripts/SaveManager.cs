using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public int counter = 0;
    public string saveDate = "";
    public string userId = "";
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "data.pem");

    public int CurrentCounter { get; private set; }
    public string CurrentUserId { get; private set; }

    private byte[] _aesKey;
    private float autoSaveTimer;
    private const float AUTO_SAVE_INTERVAL = 300f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;

        _aesKey = DeriveKey(SystemInfo.deviceUniqueIdentifier);
        Load();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Update()
    {
        autoSaveTimer += Time.deltaTime;

        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            autoSaveTimer = 0f;
            Save();
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }

    // On mobile, OnApplicationPause(true) = app sent to background → save
    void OnApplicationPause(bool pause)
    {
        if (pause)
            Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Save();
    }

    public void Save()
    {
        SaveData data = new SaveData
        {
            counter = CurrentCounter,
            userId = CurrentUserId,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")
        };

        string json = JsonUtility.ToJson(data, true);
        byte[] encrypted = Encrypt(json, _aesKey);

        string dir = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(FilePath, encrypted);

        Debug.Log("[SaveManager] Data saved.");
        Load();
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Reset();
            return;
        }

        byte[] encrypted = File.ReadAllBytes(FilePath);
        string json = Decrypt(encrypted, _aesKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        CurrentCounter = data.counter;
        CurrentUserId = data.userId;

        Debug.Log("[SaveManager] Data loaded. (counter: " + data.counter + " - Player ID: " + data.userId + ")");
    }

    public void Increment()
    {
        CurrentCounter++;
    }

    public void Reset()
    {
        CurrentCounter = 0;
        CurrentUserId = System.Guid.NewGuid().ToString();
        Debug.Log("[SaveManager] Reset data.");
        Save();
    }

    private static byte[] Encrypt(string text, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                    sw.Write(text);

                return ms.ToArray();
            }
        }
    }

    private static string Decrypt(byte[] data, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            byte[] iv = new byte[16];
            byte[] cipher = new byte[data.Length - 16];

            System.Array.Copy(data, 0, iv, 0, 16);
            System.Array.Copy(data, 16, cipher, 0, cipher.Length);

            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream(cipher))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
                return sr.ReadToEnd();
        }
    }

    private static byte[] DeriveKey(string id)
    {
        using (SHA256 sha = SHA256.Create())
            return sha.ComputeHash(Encoding.UTF8.GetBytes("SovietCliquer_v1_" + id));
    }
}