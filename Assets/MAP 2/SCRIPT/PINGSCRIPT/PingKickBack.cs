using UnityEngine;
using System.Collections;

public class Knock : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackDuration = 0.5f; // Duration of the slide

    [Header("Slide Settings")]
    public AnimationCurve slideCurve = null; // Optional custom curve for slide easing

    [Header("Animation")]
    public string backFallParameter = "BackFall"; // Name of the animator parameter for back fall
    public bool resetOnComplete = true; // Reset animation state when knockback ends

    [Header("Target Location")]
    public Transform knockbackTarget;  // Assign your cube here
    public bool useSpecificLocation = true;  // If false, uses distance-based knockback

    [Header("Distance Knockback (if useSpecificLocation = false)")]
    public float knockbackDistance = 5f;

    private void Start()
    {
        // Create a default slide curve if none is assigned
        if (slideCurve == null)
        {
            slideCurve = new AnimationCurve();
            slideCurve.AddKey(0, 0);
            slideCurve.AddKey(0.5f, 0.5f);
            slideCurve.AddKey(1, 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                if (useSpecificLocation && knockbackTarget != null)
                {
                    StartCoroutine(KnockbackToLocationSlide(other.transform, controller, knockbackTarget.position));
                }
                else
                {
                    StartCoroutine(KnockbackDirectionalSlide(other.transform, controller));
                }
            }
        }
    }

    // Helper method to set BackFall animation state
    private void SetBackFallAnimation(Transform player, bool isBackFalling)
    {
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool(backFallParameter, isBackFalling);
            Debug.Log($"Set BackFall to: {isBackFalling} on {player.name}");
        }
        else
        {
            Debug.LogWarning($"No Animator found on {player.name}");
        }
    }

    // Get ground height at a position (maintain Y position on ground)
    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 10f))
        {
            return hit.point.y;
        }
        return position.y; // Return current Y if no ground found
    }

    // Slide knockback to specific location (ground-based)
    IEnumerator KnockbackToLocationSlide(Transform player, CharacterController controller, Vector3 targetLocation)
    {
        Vector3 startPos = player.position;
        Vector3 endPos = targetLocation;

        // Get ground Y position to maintain consistent height
        float groundY = GetGroundHeight(startPos);

        // Keep Y position consistent on ground
        startPos.y = groundY;
        endPos.y = groundY;

        float elapsed = 0f;

        // Set BackFall animation to true when knockback starts
        SetBackFallAnimation(player, true);

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            // Use curve for smooth sliding
            float curveValue = slideCurve.Evaluate(t);

            // Linear interpolation for sliding on ground
            Vector3 newPosition = Vector3.Lerp(startPos, endPos, curveValue);

            // Ensure Y position stays on ground
            newPosition.y = groundY;

            // Apply movement
            controller.enabled = false;
            player.position = newPosition;
            controller.enabled = true;

            yield return null;
        }

        // Ensure final position is exactly the target on ground
        Vector3 finalPosition = new Vector3(targetLocation.x, groundY, targetLocation.z);

        controller.enabled = false;
        player.position = finalPosition;
        controller.enabled = true;

        // Reset BackFall animation when knockback ends
        if (resetOnComplete)
        {
            SetBackFallAnimation(player, false);
        }
    }

    // Directional slide knockback (away from object)
    IEnumerator KnockbackDirectionalSlide(Transform player, CharacterController controller)
    {
        Vector3 startPos = player.position;

        // Get ground Y position
        float groundY = GetGroundHeight(startPos);
        startPos.y = groundY;

        // Calculate direction away from the knockback source
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 endPos = startPos + direction * knockbackDistance;
        endPos.y = groundY;

        float elapsed = 0f;

        // Set BackFall animation to true when knockback starts
        SetBackFallAnimation(player, true);

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            // Use curve for smooth sliding
            float curveValue = slideCurve.Evaluate(t);

            // Linear interpolation for sliding on ground
            Vector3 newPosition = Vector3.Lerp(startPos, endPos, curveValue);

            // Ensure Y position stays on ground
            newPosition.y = groundY;

            // Apply movement
            controller.enabled = false;
            player.position = newPosition;
            controller.enabled = true;

            yield return null;
        }

        // Ensure final position is exactly the target
        Vector3 finalPosition = new Vector3(endPos.x, groundY, endPos.z);

        controller.enabled = false;
        player.position = finalPosition;
        controller.enabled = true;

        // Reset BackFall animation when knockback ends
        if (resetOnComplete)
        {
            SetBackFallAnimation(player, false);
        }
    }
}