using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Prefab")]
    [Tooltip("Drag your Player / XR Rig prefab from the Project folder here.")]
    public GameObject playerPrefab;

    [Header("Door Tracking")]
    // Index 0 = Door 0, Index 1 = Door 1, Index 2 = Door 2, Index 3 = Door 3, Index 4 = Door 5
    public bool[] completedDoors = new bool[5];

    [Tooltip("-1 = Starting / Reload location, 0-4 = Spawn at corresponding Door SpawnPoint")]
    public int lastDoorUsed = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Checks if the first 4 doors (indices 0, 1, 2, and 3) have all been completed.
    /// </summary>
    public bool AreFirst4DoorsCompleted()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!completedDoors[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// Resets progress/last door used to restart fresh at the default starting location.
    /// </summary>
    public void RestartGame()
    {
        lastDoorUsed = -1;
        System.Array.Clear(completedDoors, 0, completedDoors.Length);
        SceneManager.LoadScene("HubScene"); // Change to your Hub scene name
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayerAtDoor();
    }

    private void SpawnPlayerAtDoor()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("GameManager: Player Prefab is not assigned in the Inspector!");
            return;
        }

        DoorSpawnPoint[] spawnPoints = FindObjectsOfType<DoorSpawnPoint>();
        Transform targetSpawn = null;

        // Search for the spawn point matching lastDoorUsed (-1 for start, 0 for Door 0, etc.)
        foreach (var spawn in spawnPoints)
        {
            if (spawn.doorID == lastDoorUsed)
            {
                targetSpawn = spawn.transform;
                break;
            }
        }

        // Fallback: If no matching door ID is found, default to the -1 starting spawn point
        if (targetSpawn == null && lastDoorUsed != -1)
        {
            Debug.LogWarning($"GameManager: SpawnPoint for doorID {lastDoorUsed} not found in {SceneManager.GetActiveScene().name}. Falling back to default starting spawn (-1).");
            foreach (var spawn in spawnPoints)
            {
                if (spawn.doorID == -1)
                {
                    targetSpawn = spawn.transform;
                    break;
                }
            }
        }

        // Instantiate the player prefab at the target spawn location
        if (targetSpawn != null)
        {
            Instantiate(playerPrefab, targetSpawn.position, targetSpawn.rotation);
        }
        else
        {
            Debug.LogError($"GameManager: No DoorSpawnPoint found in {SceneManager.GetActiveScene().name}!");
        }
    }
}