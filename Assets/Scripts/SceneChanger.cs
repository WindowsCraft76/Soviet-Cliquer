using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    [Header("Loading screen")]
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float minimumLoadingTime = 0.5f;

    public static event Action<float> OnLoadingProgress;
    public static event Action OnLoadingStarted;
    public static event Action OnLoadingFinished;

    private string targetSceneName;
    private int targetSceneIndex = -1;
    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ChangeScene(string sceneName)
    {
        if (isLoading) return;

        targetSceneName = sceneName;
        targetSceneIndex = -1;
        StartCoroutine(LoadSceneRoutine());
    }

    public void ChangeSceneByIndex(int sceneIndex)
    {
        if (isLoading) return;

        targetSceneIndex = sceneIndex;
        targetSceneName = null;
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        isLoading = true;

        yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);

        OnLoadingStarted?.Invoke();

        float startTime = Time.time;

        AsyncOperation operation = targetSceneIndex >= 0
            ? SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Single)
            : SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float normalizedProgress = Mathf.Clamp01(operation.progress / 0.9f);
            OnLoadingProgress?.Invoke(normalizedProgress);
            yield return null;
        }

        OnLoadingProgress?.Invoke(1f);

        float elapsed = Time.time - startTime;
        if (elapsed < minimumLoadingTime)
        {
            yield return new WaitForSeconds(minimumLoadingTime - elapsed);
        }

        OnLoadingFinished?.Invoke();

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        isLoading = false;
        targetSceneName = null;
        targetSceneIndex = -1;
    }
}