using System.Collections;
using TMPro;
using UnityEngine;

public class UIButtonClick : MonoBehaviour
{
    [Header("Counter")]
    public TMP_Text counterText;
    public string defaultText = "Click on logo!";

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip   clickSound;

    [Header("Animation (optional)")]
    public RectTransform button;
    public float clickScale        = 0.9f;
    public float animationDuration = 0.1f;

    private Vector3   _originalScale;
    private Coroutine _animCoroutine;

    void Start()
    {
        if (button != null)
            _originalScale = button.localScale;

        if (counterText == null)
            Debug.LogError("[UIButtonClick] counterText is not assigned in the inspector.");

        if (SaveManager.Instance == null)
            Debug.LogWarning("[UIButtonClick] SaveManager not found.");

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
            Debug.LogWarning("[UIButtonClick] Cannot increment: SaveManager missing.");
        }

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (button != null)
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
            Debug.LogError("[UIButtonClick] counterText is not assigned in the inspector.");
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.CurrentCounter > 0)
            counterText.text = SaveManager.Instance.CurrentCounter.ToString();
        else
            counterText.text = defaultText;
    }

    private IEnumerator AnimateButton()
    {
        button.localScale = _originalScale * clickScale;
        yield return new WaitForSeconds(animationDuration);
        button.localScale = _originalScale;
        _animCoroutine = null;
    }
}