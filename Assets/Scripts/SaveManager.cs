using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

    private static string FilePath
    {
        get
        {
            string roamingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roamingPath, ".Soviet-Cliquer", "data.pem");
        }
    }

    [HideInInspector]
    public int currentCounter = 0;

    [HideInInspector]
    public string currentUserId = "";
    private byte[] _aesKey;
    private const int IV_SIZE = 16;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _aesKey = DeriveKey(SystemInfo.deviceUniqueIdentifier);
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

    void OnApplicationQuit()
    {
        Save();
        Debug.Log("[SaveManager] Save on quit.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Save();
        Debug.Log($"[SaveManager] Save on scene load: {scene.name}");
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
            userId = currentUserId,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")
        };

        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            byte[] encrypted = Encrypt(json, _aesKey);

            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(FilePath, encrypted);
            Debug.Log("[SaveManager] Data saved (encrypted).");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save data: {e.Message}");
        }
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("[SaveManager] No save file found. Initializing data.");
            currentCounter = 0;
            currentUserId = GenerateNewUUID();
            Save();
            return;
        }

        try
        {
            byte[] encrypted = File.ReadAllBytes(FilePath);
            string json = Decrypt(encrypted, _aesKey);

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            currentCounter = data.counter;

            if (IsValidUUID(data.userId))
            {
                currentUserId = data.userId;
            }
            else
            {
                Debug.LogError("[SaveManager] Corrupted or missing UUID in save file. Generating a new one.");
                currentUserId = GenerateNewUUID();
                Save();
            }

            Debug.Log($"[SaveManager] Data loaded. Counter: {currentCounter} | Last save: {data.saveDate}");
        }
        catch (CryptographicException e)
        {
            Debug.LogError($"[SaveManager] Decryption failed (wrong device or corrupted file): {e.Message}. Resetting.");
            ResetToDefaults();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load save data: {e.Message}. Resetting.");
            ResetToDefaults();
        }
    }

    public void Reset()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);

        ResetToDefaults();
        Debug.Log("[SaveManager] Save data reset.");
    }

    private static byte[] Encrypt(string plainText, byte[] key)
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
                {
                    sw.Write(plainText);
                }

                return ms.ToArray();
            }
        }
    }

    private static string Decrypt(byte[] cipherBytes, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            byte[] iv = new byte[IV_SIZE];
            byte[] cipher = new byte[cipherBytes.Length - IV_SIZE];
            System.Array.Copy(cipherBytes, 0, iv, 0, IV_SIZE);
            System.Array.Copy(cipherBytes, IV_SIZE, cipher, 0, cipher.Length);
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream(cipher))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    private static byte[] DeriveKey(string deviceId)
    {
        string salted = "SovietCliquer_v1_" + deviceId;
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(salted));
        }
    }


    private void ResetToDefaults()
    {
        currentCounter = 0;
        currentUserId = GenerateNewUUID();
        Save();
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