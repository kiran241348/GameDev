using UnityEngine;

public class ResetPositionOnTouch : MonoBehaviour
{
    [Header("Setup")]
    public Transform targetBox; // Assign Box2's Transform in Inspector
    public string playerTag = "Player";

    void Start()
    {
        // Make sure collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Add a collider if none exists
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched is the player
        if (other.CompareTag(playerTag))
        {
            // Check if target box is assigned
            if (targetBox != null)
            {
                // METHOD 1: Direct position change
                other.transform.position = targetBox.position;

                // METHOD 2: Also try setting via Transform component
                Transform playerTransform = other.GetComponent<Transform>();
                if (playerTransform != null)
                {
                    playerTransform.position = targetBox.position;
                }

                // METHOD 3: If player has a CharacterController, use it
                CharacterController controller = other.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                    other.transform.position = targetBox.position;
                    controller.enabled = true;
                }

                // METHOD 4: If player has a Rigidbody, set position properly
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.position = targetBox.position;
                    rb.linearVelocity = Vector3.zero;
                }

                // Double check if position actually changed
                Debug.Log($"Player reset to: {targetBox.name} at position {targetBox.position}");
                Debug.Log($"Player is now at: {other.transform.position}");
            }
            else
            {
                Debug.LogError("Target Box not assigned! Please drag Box2 into the script.");
            }
        }
    }

    // Visual helper in editor
    void OnDrawGizmos()
    {
        if (targetBox != null)
        {
            // Draw line from Box1 to Box2
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetBox.position);

            // Draw sphere at Box2 position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetBox.position, 0.5f);
        }

        // Draw Box1's collider
        Gizmos.color = Color.red;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}