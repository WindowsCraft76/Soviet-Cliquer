using UnityEngine;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Name of the first scene to load")]
    [SerializeField] private string firstSceneName;

    private void Start()
    {
        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("[BootstrapLoader] No scene set in the Inspector.");
            return;
        }

        if (SceneChanger.Instance == null)
        {
            Debug.LogError("[BootstrapLoader] SceneChanger.Instance is null.");
            return;
        }

        SceneChanger.Instance.ChangeScene(firstSceneName);
    }
}