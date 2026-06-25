using UnityEngine;
using TMPro;

public class MenuUIDDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uidText;

    void Start()
    {
        DisplayUUID();
    }

    public void DisplayUUID()
    {
        if (SaveManager.Instance != null && uidText != null)
        {
            uidText.text = SaveManager.Instance.currentUserId;
        }
        else if (uidText == null)
        {
            Debug.LogError("[MenuUIDDisplay] The TextMeshProUGUI component is not assigned in the inspector!");
        }
    }
}