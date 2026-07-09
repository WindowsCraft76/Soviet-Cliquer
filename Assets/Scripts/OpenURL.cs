using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public void Open(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("Empty URL.");
            return;
        }

        Application.OpenURL(url);
    }
}