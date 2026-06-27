using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        SaveManager.Instance.Save();
        Debug.Log("Close game.");
        Application.Quit();
    }
}