using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class VersionManager : MonoBehaviour
{

    [Header("Configuration")]
    [Tooltip("Version API URL")]
    [SerializeField] private string apiUrl;

    public static event Action OnLocalVersionReady;

    public static string LocalDisplayVersion => s_localDisplayVersion;

    public static event Action<bool> OnUpdateAvailable;

    public static bool HasCheckedForUpdate => s_fetchDone;
    public static bool IsUpdateAvailable => s_updateAvailable;
    public static bool IsFirstInstall => s_isFirstInstall;

    private static string s_localRawVersion      = null;
    private static string s_localDisplayVersion   = null;
    private static string s_remoteVersion         = null;
    private static string s_remoteDisplayVersion  = null;
    private static bool   s_updateAvailable       = false;
    private static bool   s_isFirstInstall        = false;
    private static bool   s_fetchDone             = false;

    private void Start()
    {
        if (s_localRawVersion == null)
        {
            // Application.version is set in Unity Player Settings → Version field.
            // It works on Android, iOS, and all other platforms.
            s_localRawVersion    = ReadLocalVersion();
            s_localDisplayVersion = FormatLocalVersionForDisplay(s_localRawVersion);
            OnLocalVersionReady?.Invoke();
        }

        if (s_fetchDone)
        {
            ApplyCachedResult();
        }
        else
        {
            StartCoroutine(FetchRemoteVersion());
        }
    }

    private void ApplyCachedResult()
    {
        if (s_updateAvailable)
            NotifyUpdate(s_isFirstInstall);
    }

    private string ReadLocalVersion()
    {
        return Application.version;
    }

    private IEnumerator FetchRemoteVersion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[VersionManager] Unable to connect to the API: {request.error}");
                yield break;
            }

            ParseAndCompare(request.downloadHandler.text);
            s_fetchDone = true;

            ApplyCachedResult();
        }
    }

    [Serializable] private class ApiResponse { public LastBlock last; }
    [Serializable] private class LastBlock  { public string version; public string type; public BetaBlock beta; }
    [Serializable] private class BetaBlock  { public string version; public string type; }

    private static string TypeToSuffix(string type)
    {
        if (string.IsNullOrEmpty(type)) return string.Empty;
        switch (type.ToLowerInvariant())
        {
            case "release": return "r";
            case "hotfix":  return "h";
            case "beta":    return "b";
            default:        return string.Empty;
        }
    }

    private void ParseAndCompare(string json)
    {
        ApiResponse response;
        try   { response = JsonUtility.FromJson<ApiResponse>(json); }
        catch (Exception ex)
        {
            Debug.LogError($"[VersionManager] JSON parsing error: {ex.Message}");
            return;
        }

        if (response?.last == null)
        {
            Debug.LogWarning("[VersionManager] Invalid API response ('last' field missing).");
            return;
        }

        bool hasRelease = !string.IsNullOrEmpty(response.last.version);

        string rawNumber, suffix;
        if (hasRelease)
        {
            rawNumber = response.last.version;
            suffix    = TypeToSuffix(response.last.type);
        }
        else
        {
            rawNumber = response.last.beta?.version ?? string.Empty;
            suffix    = TypeToSuffix(response.last.beta?.type);
        }

        string cleanNumber        = Regex.Replace(rawNumber.TrimStart('v', 'V'), @"[a-zA-Z]+$", string.Empty).Trim();
        s_remoteVersion           = cleanNumber;
        s_remoteDisplayVersion    = $"v{cleanNumber}{suffix}";

        if (string.IsNullOrEmpty(s_remoteVersion))
        {
            Debug.LogWarning("[VersionManager] No version available in the API.");
            return;
        }

        Debug.Log($"[VersionManager] Local: '{s_localRawVersion}' | Remote: '{s_remoteVersion}'");
        CompareVersions();
    }

    private static string StripVersionDecorators(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        string s = raw.TrimStart('v', 'V');
        return Regex.Replace(s, @"[a-zA-Z]+$", string.Empty).Trim();
    }

    private void CompareVersions()
    {
        string localNumeric  = StripVersionDecorators(s_localRawVersion);
        string remoteNumeric = s_remoteVersion ?? string.Empty;

        if (string.IsNullOrEmpty(localNumeric))
        {
            s_updateAvailable = true;
            s_isFirstInstall  = true;
            return;
        }

        if (!Version.TryParse(localNumeric,  out Version localVer))  return;
        if (!Version.TryParse(remoteNumeric, out Version remoteVer)) return;

        if (localVer < remoteVer)
        {
            Debug.Log($"[VersionManager] Update available: {s_localRawVersion} → {s_remoteVersion}");
            s_updateAvailable = true;
            s_isFirstInstall  = false;
        }
        else
        {
            Debug.Log("[VersionManager] The game is up to date.");
            s_updateAvailable = false;
        }
    }

    private void NotifyUpdate(bool isFirstInstall)
    {
        OnUpdateAvailable?.Invoke(isFirstInstall);
    }

    private static string GetLocalSuffix(string numericVersion)
    {
        if (string.IsNullOrEmpty(numericVersion)) return string.Empty;

        string[] parts = numericVersion.Split('.');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int patch) && patch > 0)
            return "h";
        if (parts.Length >= 2 && int.TryParse(parts[1], out int minor) && minor > 0)
            return "b";
        return "r";
    }

    private static string FormatLocalVersionForDisplay(string numericVersion)
    {
        if (string.IsNullOrEmpty(numericVersion)) return "Version not found!";
        return $"v{numericVersion}{GetLocalSuffix(numericVersion)}";
    }

}