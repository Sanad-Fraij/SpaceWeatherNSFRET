using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    private static PlayerInstance instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // A persistent player already exists from another scene, so destroy this duplicate
            Destroy(gameObject);
        }
    }
}