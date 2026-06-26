using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        SaveManager.Instance.Reset();
        Debug.Log("Close game.");
        Application.Quit();
    }
}