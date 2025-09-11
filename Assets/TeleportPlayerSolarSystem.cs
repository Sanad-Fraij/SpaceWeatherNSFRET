using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerSolarSystem : MonoBehaviour
{
    [Header("Stylized Astronaut")]
    public Transform player;

    [Header("TPDestination")]
    public Transform TPDestination;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing that entered the trigger is the player
        if (other.transform == player)
        {
            // Teleport player to the destination
            player.position = TPDestination.position;
            player.rotation = TPDestination.rotation; // Optional: match rotation too
        }
    }
}
