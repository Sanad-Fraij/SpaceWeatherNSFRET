using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DoorIndicator : MonoBehaviour
{
    [Header("Door")]
    [Tooltip("Index into GameManager.completedDoors (0-4). Must match the door's ID.")]
    public int doorNumber = 0;

    [Header("Materials")]
    public Material redMaterial;
    public Material greenMaterial;

    [Tooltip("Which material slot on the renderer to swap (usually 0).")]
    public int materialIndex = 0;

    private Renderer rend;
    private bool? lastState = null; // null = not applied yet

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        // Cheap check so the indicator updates live if a door is completed while this scene is open
        Refresh();
    }

    private void Refresh()
    {
        bool cleared = IsDoorCleared();

        if (lastState == cleared) return;
        lastState = cleared;

        Material[] mats = rend.sharedMaterials;
        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
            Debug.LogWarning($"DoorIndicator on {name}: materialIndex {materialIndex} is out of range.");
            return;
        }

        mats[materialIndex] = cleared ? greenMaterial : redMaterial;
        rend.sharedMaterials = mats;
    }

    private bool IsDoorCleared()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.completedDoors == null) return false;

        if (doorNumber < 0 || doorNumber >= gm.completedDoors.Length)
        {
            Debug.LogWarning($"DoorIndicator on {name}: doorNumber {doorNumber} is out of range.");
            return false;
        }

        return gm.completedDoors[doorNumber];
    }
}