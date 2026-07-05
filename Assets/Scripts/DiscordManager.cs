using DiscordRPC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordManager : MonoBehaviour
{
    public static DiscordManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private string applicationId;

    [Header("Image")]
    [SerializeField] private string largeImageKey;

    [Header("Button")]
    [SerializeField] private string buttonLabel;
    [SerializeField] private string buttonUrl;

    private DiscordRpcClient client;
    private Timestamps sessionTimestamp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;

        client = new DiscordRpcClient(applicationId);
        client.Logger = new DiscordRPC.Logging.ConsoleLogger(DiscordRPC.Logging.LogLevel.Trace, true);
        client.Initialize();

        sessionTimestamp = Timestamps.Now;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private const float UPDATE_COOLDOWN = 16f;
    private const float CLICK_CHECK_INTERVAL = 60f;
    private const float INACTIVITY_THRESHOLD = 120f;

    private float lastUpdateTime = -999f;
    private float lastClickCheckTime = -999f;
    private float lastClickChangeTime = -999f;

    private int lastKnownClickCount = -1;
    private bool isTakingBreak;

    private string pendingDetails;
    private string pendingState;
    private bool hasPendingUpdate;

    private string currentSceneName;

    private void Update()
    {
        client?.Invoke();

        if (hasPendingUpdate && Time.unscaledTime - lastUpdateTime >= UPDATE_COOLDOWN)
        {
            ApplyPresence(pendingDetails, pendingState);
        }

        if (currentSceneName == "Game" && Time.unscaledTime - lastClickCheckTime >= CLICK_CHECK_INTERVAL)
        {
            CheckClickActivity();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        string details = GetDetailsForScene(scene.name);
        if (details == null) return;

        if (scene.name == "Game")
        {
            lastKnownClickCount = SaveManager.Instance.CurrentCounter;
            lastClickChangeTime = Time.unscaledTime;
            lastClickCheckTime = Time.unscaledTime;
            isTakingBreak = false;

            SetPresence(details, GetClickState());
        }
        else
        {
            SetPresence(details, null);
        }
    }

    private string GetDetailsForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Menu": return "In the menu";
            case "Game": return "Is clicking...";
            case "LoadingScreen": return null;
            case "Bootstrap": return null;
            default: return sceneName;
        }
    }

    private string GetClickState()
    {
        return SaveManager.Instance.CurrentCounter + " clicks";
    }

    private void CheckClickActivity()
    {
        lastClickCheckTime = Time.unscaledTime;

        int currentCount = SaveManager.Instance.CurrentCounter;

        if (currentCount != lastKnownClickCount)
        {
            lastKnownClickCount = currentCount;
            lastClickChangeTime = Time.unscaledTime;

            if (isTakingBreak)
            {
                isTakingBreak = false;
            }
        }
        else if (!isTakingBreak && Time.unscaledTime - lastClickChangeTime >= INACTIVITY_THRESHOLD)
        {
            isTakingBreak = true;
        }

        string details = isTakingBreak ? "Take a break..." : "Is clicking...";
        SetPresence(details, GetClickState());
    }

    private void SetPresence(string details, string state)
    {
        pendingDetails = details;
        pendingState = state;
        hasPendingUpdate = true;

        if (Time.unscaledTime - lastUpdateTime >= UPDATE_COOLDOWN)
        {
            ApplyPresence(details, state);
        }
        else
        {
            Debug.Log($"[DiscordManager] Update queued...");
        }
    }

    private void ApplyPresence(string details, string state)
    {
        if (client == null || client.IsDisposed) return;

        client.SetPresence(new RichPresence
        {
            Details = details,
            State = state,
            Assets = new Assets
            {
                LargeImageKey = largeImageKey,
                LargeImageText = VersionManager.LocalDisplayVersion
            },
            Timestamps = sessionTimestamp,
            Buttons = !string.IsNullOrEmpty(buttonLabel) && !string.IsNullOrEmpty(buttonUrl)
                ? new[] { new global::DiscordRPC.Button { Label = buttonLabel, Url = buttonUrl } }
                : null
        });

        Debug.Log($"[DiscordManager] Update applied");

        lastUpdateTime = Time.unscaledTime;
        hasPendingUpdate = false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        client?.Dispose();
    }
}