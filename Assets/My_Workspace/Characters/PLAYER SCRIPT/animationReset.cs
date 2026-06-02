using UnityEngine;

public class ResetAnimatorOnTouch : MonoBehaviour
{
    // Assign your player GameObject in the Inspector
    public GameObject player;

    private Animator playerAnimator;

    void Start()
    {
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player touched this object
        if (other.gameObject == player && playerAnimator != null)
        {
            ResetAnimator();
        }
    }

    private void ResetAnimator()
    {
        // Reset all animator parameters
        playerAnimator.Rebind();   // Rebind resets the Animator to default values
        playerAnimator.Update(0f); // Forces immediate update

        // Optionally trigger a spawn animation
        playerAnimator.SetTrigger("Spawn");

        Debug.Log("Player Animator has been reset!");
    }
}
