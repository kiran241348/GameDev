using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLaunch2 : MonoBehaviour
{
    private CharacterController controller;

    private Vector3 launchVelocity;
    private bool isLaunched;

    [Header("Settings")]
    public float landingDelay = 0.2f;

    private float launchTimer;

    public bool IsLaunched => isLaunched;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isLaunched)
            return;

        launchTimer += Time.deltaTime;

        // Apply gravity
        launchVelocity += Physics.gravity * Time.deltaTime;

        // Move player
        controller.Move(launchVelocity * Time.deltaTime);

        // Prevent instant landing detection
        if (launchTimer > landingDelay &&
            controller.isGrounded &&
            launchVelocity.y <= 0f)
        {
            isLaunched = false;
            launchVelocity = Vector3.zero;
        }
    }

    public void Launch(Vector3 velocity)
    {
        launchVelocity = velocity;
        launchTimer = 0f;
        isLaunched = true;
    }

    public void StopLaunch()
    {
        isLaunched = false;
        launchVelocity = Vector3.zero;
    }
}