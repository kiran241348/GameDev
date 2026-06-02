using UnityEngine;
using System.Collections;

public class KnockbackPlate : MonoBehaviour
{
    public float knockbackDistance = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Animator Settings")]
    public float backFallDuration = 0.5f; // How long to keep BackFall true

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            Animator animator = other.GetComponent<Animator>();

            if (controller != null)
            {
                StartCoroutine(KnockbackPlayer(other.transform, controller, animator));
            }
        }
    }

    IEnumerator KnockbackPlayer(Transform player, CharacterController controller, Animator animator)
    {
        Vector3 startPos = player.position;

        // Direction from plate to player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // If player is behind plate, push forward instead
        if (Vector3.Dot(direction, transform.forward) < 0)
        {
            direction = transform.forward;
        }

        Vector3 targetPos = startPos + direction.normalized * knockbackDistance;

        float elapsed = 0f;

        // Trigger BackFall animation
        if (animator != null)
        {
            // Reset other states
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsGrounded", true);

            // Set BackFall to true
            animator.SetBool("BackFall", true);

            if (animator.GetBool("IsReSpawn") == true)
            {
                animator.SetBool("IsReSpawn", false);
            }

            Debug.Log("BackFall animation triggered!");
        }

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 move = Vector3.Lerp(startPos, targetPos, elapsed / knockbackDuration);

            controller.enabled = false;
            player.position = move;
            controller.enabled = true;

            yield return null;
        }

        // Wait before turning off BackFall animation
        yield return new WaitForSeconds(backFallDuration);

        // Turn off BackFall animation
        if (animator != null)
        {
            animator.SetBool("BackFall", false);
            animator.SetBool("IsGrounded", true);
            Debug.Log("BackFall animation ended!");
        }
    }
}