using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public CharacterController player; // Or use GameObject if you don't use CharacterController
    public Transform destination;

    private Vector3 defaultPosition;

    void Start()
    {
        defaultPosition = player.transform.position;
    }

    // Button 1
    public void TeleportToDestination()
    {
        player.enabled = false;
        player.transform.position = destination.position;
        player.enabled = true;
    }

    // Button 2
    public void ResetToDefaultPosition()
    {
        player.enabled = false;
        player.transform.position = defaultPosition;
        player.enabled = true;
    }
}