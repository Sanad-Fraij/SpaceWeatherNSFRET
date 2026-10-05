using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorButton : MonoBehaviour
{
    [Tooltip("Name of the 5th scene to load when all 4 doors are completed")]
    public string scene5Name = "Scene5";

    [Tooltip("The Door ID to assign for spawn positioning upon loading Scene 5 (typically 4 for Door 5)")]
    public int door5ID = 4;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        // Check if an XR Interactable component exists on this button
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            // Subscribe to XR Controller activation (e.g. pressing trigger/action button while aiming/hovering)
            interactable.activated.AddListener(OnXRActivated);
            // Optional: Subscribe to select/grab event if you want it to trigger on direct touch/grab
            interactable.selectEntered.AddListener(OnXRSelect);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.activated.RemoveListener(OnXRActivated);
            interactable.selectEntered.RemoveListener(OnXRSelect);
        }
    }

    private void OnXRActivated(ActivateEventArgs args)
    {
        OnButtonPressed();
    }

    private void OnXRSelect(SelectEnterEventArgs args)
    {
        OnButtonPressed();
    }

    /// <summary>
    /// Core button logic. Can be called from UI, Physics Triggers, or XR Events.
    /// </summary>
    public void OnButtonPressed()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("DoorButton: GameManager Instance not found!");
            return;
        }

        if (GameManager.Instance.AreFirst4DoorsCompleted())
        {
            Debug.Log("All 4 doors completed! Loading Scene 5...");
            GameManager.Instance.lastDoorUsed = door5ID;
            SceneManager.LoadScene(scene5Name);
        }
        else
        {
            Debug.Log("Button locked! You must complete Doors 0, 1, 2, and 3 first.");
        }
    }

    // Physical collision trigger (walking into it or moving a collider through it)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || 
            other.GetComponentInParent<CharacterController>() != null || 
            other.GetComponentInParent<XRBaseController>() != null)
        {
            OnButtonPressed();
        }
    }
}