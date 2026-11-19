using UnityEngine;

public class DoorBoardIndicator : MonoBehaviour
{
    [System.Serializable]
    public class BoardEntry
    {
        [Tooltip("The child object's renderer to recolor.")]
        public Renderer target;

        [Tooltip("Index into GameManager.completedDoors (0-4). The last door is 4, not 5.")]
        public int doorNumber = 0;

        [Tooltip("Which material slot on the renderer to swap (usually 0).")]
        public int materialIndex = 0;

        [HideInInspector] public bool? lastState = null; // null = not applied yet
    }

    [Header("Materials")]
    public Material redMaterial;
    public Material greenMaterial;

    [Header("Board Entries")]
    public BoardEntry[] entries;

    private void OnEnable()
    {
        RefreshAll();
    }

    private void Update()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        if (entries == null) return;

        foreach (BoardEntry entry in entries)
        {
            if (entry == null || entry.target == null) continue;

            bool cleared = IsDoorCleared(entry.doorNumber);

            if (entry.lastState == cleared) continue;
            entry.lastState = cleared;

            Material[] mats = entry.target.sharedMaterials;
            if (entry.materialIndex < 0 || entry.materialIndex >= mats.Length)
            {
                Debug.LogWarning($"DoorBoardIndicator on {name}: materialIndex {entry.materialIndex} is out of range for {entry.target.name}.");
                continue;
            }

            mats[entry.materialIndex] = cleared ? greenMaterial : redMaterial;
            entry.target.sharedMaterials = mats;
        }
    }

    private bool IsDoorCleared(int doorNumber)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.completedDoors == null) return false;

        if (doorNumber < 0 || doorNumber >= gm.completedDoors.Length)
        {
            Debug.LogWarning($"DoorBoardIndicator on {name}: doorNumber {doorNumber} is out of range.");
            return false;
        }

        return gm.completedDoors[doorNumber];
    }
}