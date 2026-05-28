using UnityEngine;

public class CameraJumpSmooth : MonoBehaviour
{
    public Transform player;

    [Header("Jump Smooth")]
    public float smoothSpeed = 5f;
    public float jumpEffectAmount = 0.2f;

    private float originalOffsetY;
    private float currentY;

    void Start()
    {
        // Store original camera height difference
        originalOffsetY = transform.position.y - player.position.y;

        currentY = transform.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Keep original camera height
        float targetY =
            player.position.y
            + originalOffsetY;

        // Follow only a little amount of jump
        targetY = Mathf.Lerp(
            transform.position.y,
            targetY,
            jumpEffectAmount
        );

        // Smooth vertical movement
        currentY = Mathf.Lerp(
            currentY,
            targetY,
            smoothSpeed * Time.deltaTime
        );

        // ONLY modify vertical height
        transform.position = new Vector3(
            transform.position.x,
            currentY,
            transform.position.z
        );
    }
}