using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private TextMeshProUGUI progressText;

    private void OnEnable()
    {
        SceneChanger.OnLoadingProgress += HandleProgress;
    }

    private void OnDisable()
    {
        SceneChanger.OnLoadingProgress -= HandleProgress;
    }

    private void HandleProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressFillImage != null)
            progressFillImage.fillAmount = progress;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}