using UnityEngine;
using UnityEngine.UI;

public class ResetDataButtonLinker : MonoBehaviour
{
    [SerializeField] private Button ResetData;

    private void Start()
    {
        if (ResetData == null)
        {
            Debug.LogError("[ResetButtonLinker] ResetData button not assigned!");
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[ResetButtonLinker] SaveManager instance not found!");
            return;
        }

        ResetData.onClick.AddListener(() => SaveManager.Instance.Reset());
    }
}