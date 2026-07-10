using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Configuration")]
    public Button button;
    private RectTransform buttonRect;

    [Header("Audio (optional)")]
    public AudioClip   clickSound;
    private AudioSource audioSource;

    [Header("Animation (optional)")]
    public float clickScale        = 0.9f;
    public float animationDuration = 0.1f;

    [Header("Counter")]
    public TMP_Text counterText;
    public string defaultText = "Click on logo!";

    [Header("Debug")]
    [SerializeField] private bool debugModeEnabled = false;
    [SerializeField] private double debugCounterValue = 0;
    
    private Vector3   _originalScale;
    private Coroutine _animCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (button != null)
        {
            buttonRect = button.GetComponent<RectTransform>();
            _originalScale = buttonRect.localScale;

            button.onClick.AddListener(Click);
        }

        if (counterText == null)
            Debug.LogError("[GameUI] counterText is not assigned in the inspector.");

        if (SaveManager.Instance == null)
            Debug.LogWarning("[GameUI] SaveManager not found.");

        if (SettingsManager.Instance != null)
        {
            ApplyClickVolume(SettingsManager.Instance.ClickVolume);
            SettingsManager.Instance.OnClickVolumeChanged += ApplyClickVolume;
            SettingsManager.Instance.OnNumberFormatChanged += OnNumberFormatChanged;
        }

    RefreshCounter();
}

    public void Click()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Increment();
            RefreshCounter();
        }
        else
        {
            Debug.LogWarning("[GameUI] Cannot increment: SaveManager missing.");
        }

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (buttonRect != null && (SettingsManager.Instance == null || SettingsManager.Instance.AnimationsEnabled))
        {
            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            _animCoroutine = StartCoroutine(AnimateButton());
        }
    }

    private void RefreshCounter()
    {
        if (counterText == null)
        {
            Debug.LogError("[GameUI] counterText is not assigned in the inspector.");
            return;
        }

        NumberFormatType format = SettingsManager.Instance != null
            ? SettingsManager.Instance.NumberFormat
            : NumberFormatType.Separated;

        if (debugModeEnabled)
        {
            counterText.text = debugCounterValue > 0
                ? NumberFormatter.Format(debugCounterValue, format)
                : defaultText;
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.CurrentCounter > 0)
            counterText.text = NumberFormatter.Format(SaveManager.Instance.CurrentCounter, format);
        else
            counterText.text = defaultText;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && counterText != null)
            RefreshCounter();
    }

    private IEnumerator AnimateButton()
    {
        buttonRect.localScale = _originalScale * clickScale;
        yield return new WaitForSeconds(animationDuration);
        buttonRect.localScale = _originalScale;
        _animCoroutine = null;
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnClickVolumeChanged -= ApplyClickVolume;
            SettingsManager.Instance.OnNumberFormatChanged -= OnNumberFormatChanged;
        }
        if (button != null)
            button.onClick.RemoveListener(Click);
    }

    private void ApplyClickVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    private void OnNumberFormatChanged(NumberFormatType format)
    {
        RefreshCounter();
    }
}