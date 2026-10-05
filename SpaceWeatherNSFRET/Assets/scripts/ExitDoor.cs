using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Tooltip("0 = Door 0, 1 = Door 1, 2 = Door 2, 3 = Door 3, 4 = Door 5 (Final Door)")]
    public int doorID;

    [Tooltip("Name of the scene to load when entering this door")]
    public string targetSceneName;

    [Tooltip("Check this if this door is inside a sub-scene/room and returning the player to the Hub")]
    public bool isReturningToHub = false;

    private void OnTriggerEnter(Collider other)
    {
        // Detect player collision (check tag or root object)
        if (other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null)
        {
            // Lock Door 5 (index 4) if entering from Hub and first 4 doors are incomplete
            if (doorID == 4 && !isReturningToHub && !GameManager.Instance.AreFirst4DoorsCompleted())
            {
                Debug.Log("Door 5 is locked! Complete Doors 0, 1, 2, and 3 first.");
                return;
            }

            // Update last used door ID for player placement in the target scene
            GameManager.Instance.lastDoorUsed = doorID;

            // Mark door completed when returning from the room back to the hub
            if (isReturningToHub)
            {
                if (doorID >= 0 && doorID < GameManager.Instance.completedDoors.Length)
                {
                    GameManager.Instance.completedDoors[doorID] = true;
                    //Debug.Log($"Door {doorID} marked as Completed!");
                }
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}