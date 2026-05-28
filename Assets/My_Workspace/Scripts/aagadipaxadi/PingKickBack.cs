using UnityEngine;
using System.Collections;

public class Knock : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f;

    [Header("Target Location")]
    public Transform knockbackTarget;  // Assign your cube here
    public bool useSpecificLocation = true;  // If false, uses distance-based knockback

    [Header("Distance Knockback (if useSpecificLocation = false)")]
    public float knockbackDistance = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                if (useSpecificLocation && knockbackTarget != null)
                {
                    StartCoroutine(KnockbackToLocation(other.transform, controller, knockbackTarget.position));
                }
                else
                {
                    StartCoroutine(KnockbackDirectional(other.transform, controller));
                }
            }
        }
    }

    // Knockback to specific location (cube)
    IEnumerator KnockbackToLocation(Transform player, CharacterController controller, Vector3 targetLocation)
    {
        Vector3 startPos = player.position;

        // Calculate direction to target location
        Vector3 direction = (targetLocation - startPos).normalized;

        // Optional: Keep Y position? Uncomment if you want to maintain height
        // targetLocation.y = startPos.y;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 move = Vector3.Lerp(startPos, targetLocation, elapsed / knockbackDuration);

            controller.enabled = false;
            player.position = move;
            controller.enabled = true;

            yield return null;
        }

        // Ensure final position is exactly the target
        controller.enabled = false;
        player.position = targetLocation;
        controller.enabled = true;
    }

    // Original knockback based on distance
    IEnumerator KnockbackDirectional(Transform player, CharacterController controller)
    {
        Vector3 startPos = player.position;

        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 targetPos = startPos + direction * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 move = Vector3.Lerp(startPos, targetPos, elapsed / knockbackDuration);

            controller.enabled = false;
            player.position = move;
            controller.enabled = true;

            yield return null;
        }
    }
}