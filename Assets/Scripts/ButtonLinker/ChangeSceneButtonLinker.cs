using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneButtonLinker : MonoBehaviour
{
    [SerializeField] private Button button;

    [Tooltip("Name of the scene to load")]
    [SerializeField] private string sceneName;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            Debug.LogError($"[ChangeSceneButtonLinker] No Button assigned or found on {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[ChangeSceneButtonLinker] No scene name set on {gameObject.name}");
            return;
        }

        if (SceneChanger.Instance == null)
        {
            Debug.LogError("[ChangeSceneButtonLinker] SceneChanger.Instance is null.");
            return;
        }

        SceneChanger.Instance.ChangeScene(sceneName);
    }
}