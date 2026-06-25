using UnityEngine;

public class PersistentEventSystem : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectsByType<PersistentEventSystem>(FindObjectsInactive.Include).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}