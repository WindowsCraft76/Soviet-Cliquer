using TMPro;
using UnityEngine;

public class MenuUIDDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uidText;

    void Start() => DisplayUID();

    public void DisplayUID()
    {
        if (uidText == null)
        {
            Debug.LogError("[MenuUIDDisplay] TextMeshProUGUI not assigned in the inspector.");
            return;
        }

        if (SaveManager.Instance != null)
            uidText.text = SaveManager.Instance.CurrentUserId;
    }
}