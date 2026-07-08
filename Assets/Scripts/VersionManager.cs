using System;
using System.Collections;
using System.Diagnostics;
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

    private const string RegistryKeyPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SovietCliquer";

    private const string RegistryValueName = "DisplayVersion";

    private const string LinuxPackageName = "sovietcliquer";

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
#if UNITY_STANDALONE_WIN
        string result = QueryRegistry(use32BitView: false);
        if (!string.IsNullOrEmpty(result)) return result;

        result = QueryRegistry(use32BitView: true);
        if (!string.IsNullOrEmpty(result)) return result;

        Debug.LogWarning($"[VersionManager] Value '{RegistryValueName}' not found in both registry views.");
        return string.Empty;
#elif UNITY_STANDALONE_LINUX
        string result = QueryDpkg();
        if (!string.IsNullOrEmpty(result)) return result;

        result = QueryRpm();
        if (!string.IsNullOrEmpty(result)) return result;

        Debug.LogWarning($"[VersionManager] Package '{LinuxPackageName}' not found via dpkg or rpm.");
        return string.Empty;
#else
        Debug.LogWarning("[VersionManager] Local version reading is not available on this platform.");
        return string.Empty;
#endif
    }

#if UNITY_STANDALONE_WIN
    private string QueryRegistry(bool use32BitView)
    {
        try
        {
            string regFlag = use32BitView ? " /reg:32" : string.Empty;
            string args    = $@"query ""HKLM\{RegistryKeyPath}"" /v {RegistryValueName}{regFlag}";

            var psi = new ProcessStartInfo
            {
                FileName               = "reg",
                Arguments              = args,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                Debug.Log($"[VersionManager] reg query ({(use32BitView ? "32-bit" : "64-bit")}) :\n{output}");

                Match match = Regex.Match(output, @"REG_SZ\s+(\S+)");
                if (match.Success)
                    return match.Groups[1].Value.Trim();

                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VersionManager] reg query error: {ex.Message}");
            return string.Empty;
        }
    }
#endif

#if UNITY_STANDALONE_LINUX
    private string QueryDpkg()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "dpkg-query",
                Arguments              = $"-W -f=${{Version}} {LinuxPackageName}",
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;

                string output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();

                Debug.Log($"[VersionManager] dpkg-query exit={proc.ExitCode} output='{output}'");

                if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    return CleanPackageVersion(output);

                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"[VersionManager] dpkg-query indisponible : {ex.Message}");
            return string.Empty;
        }
    }

    private string QueryRpm()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "rpm",
                Arguments              = $"-q --qf %{{VERSION}} {LinuxPackageName}",
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;

                string output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();

                Debug.Log($"[VersionManager] rpm -q exit={proc.ExitCode} output='{output}'");

                if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    return CleanPackageVersion(output);

                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            // rpm absent (ex: distribution basée sur Debian) : pas une erreur bloquante.
            Debug.Log($"[VersionManager] rpm indisponible : {ex.Message}");
            return string.Empty;
        }
    }

    // "2:1.2.3-1" -> "1.2.3" (retire l'epoch éventuel et la révision de paquet fpm/dpkg/rpm)
    private static string CleanPackageVersion(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        int colonIndex = raw.IndexOf(':');
        if (colonIndex >= 0) raw = raw.Substring(colonIndex + 1);

        int dashIndex = raw.IndexOf('-');
        if (dashIndex >= 0) raw = raw.Substring(0, dashIndex);

        return raw.Trim();
    }
#endif

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