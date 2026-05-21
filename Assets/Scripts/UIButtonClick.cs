using UnityEngine;
using TMPro;
using System.Collections;

public class UIButtonClick : MonoBehaviour
{
    [Header("Counter (Optional)")]
    public TMP_Text counterText;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Animation (Optional)")]
    public RectTransform button;
    public float clickScale = 0.9f;
    public float animationDuration = 0.1f;

    [Header("Default Text")]
    public string defaultText = "Click on logo!";

    private Vector3 originalScale;

    void Start()
    {
        if (button != null)
        {
            originalScale = button.localScale;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[UIButtonClick] SaveManager not found.");
        }

        UpdateUI();
    }

    public void Click()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.currentCounter++;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("[UIButtonClick] Cannot increment: SaveManager is missing or inactive.");
        }

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (button != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimationButton());
        }
    }

    void UpdateUI()
    {
        if (counterText == null) return; 

        if (SaveManager.Instance != null)
        {
            int currentCount = SaveManager.Instance.currentCounter;
            if (currentCount == 0)
            {
                counterText.text = defaultText;
            }
            else
            {
                counterText.text = currentCount.ToString();
            }
        }
        else
        {
            counterText.text = defaultText;
        }
    }

    IEnumerator AnimationButton()
    {
        if (button != null)
        {
            button.localScale = originalScale * clickScale;
            yield return new WaitForSeconds(animationDuration);
            button.localScale = originalScale;
        }
    }
}