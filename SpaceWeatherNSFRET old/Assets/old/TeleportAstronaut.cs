using UnityEngine;

public class TeleportAstronaut : MonoBehaviour
{
    [Header("Assign the teleport destination here")]
    public Transform TPDestination;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the AstronautPlayer script
        AstronautPlayer.AstronautPlayer astronaut = other.GetComponent<AstronautPlayer.AstronautPlayer>();

        if (astronaut != null)
        {
            // Teleport astronaut
            CharacterController controller = astronaut.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Disable CharacterController temporarily to avoid conflicts
                controller.enabled = false;
                astronaut.transform.position = TPDestination.position;
                astronaut.transform.rotation = TPDestination.rotation; // optional
                controller.enabled = true;
            }
        }
    }
}
