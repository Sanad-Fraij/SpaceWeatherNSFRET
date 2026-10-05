using UnityEngine;

public class DoorSpawnPoint : MonoBehaviour
{
    [Tooltip("Set to -1 for Initial Start / Reload spawn point. Set to 0, 1, 2, 3, or 4 for specific doors.")]
    public int doorID = -1;

    private void OnDrawGizmos()
    {
        // Visual indicator in Scene view for easy setup
        Gizmos.color = (doorID == -1) ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
}