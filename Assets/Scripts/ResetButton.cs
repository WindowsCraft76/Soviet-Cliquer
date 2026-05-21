using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public void OnResetClick()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Reset();
        }
        else
        {
            Debug.LogError("[ResetButton] SaveManager not found!");
        }
    }
}