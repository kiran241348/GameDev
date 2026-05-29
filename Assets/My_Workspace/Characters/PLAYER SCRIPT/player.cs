using UnityEngine;
using UnityEngine.UI; // For UI elements if needed

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;

    // Joystick reference - drag your joystick here in the inspector
    public VariableJoystick movementJoystick; // Using VariableJoystick from Unity's Input System package
                                              // Alternative: public FixedJoystick movementJoystick; or public FloatingJoystick movementJoystick;

    // Optional: For touch input on mobile
    public bool useJoystick = true;
    public bool useKeyboardAsFallback = true; // Keep keyboard support for testing

    [Header("Movement")]
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Jump Button")]
    public Button jumpButton; // Optional: Assign a UI button for jumping

    private Vector3 velocity;
    private bool isGrounded;
    private bool hasJumped; // Tracks if player has jumped and hasn't landed yet
    private bool jumpRequested; // For mobile jump button

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Setup jump button if assigned
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(RequestJump);
        }

        // REMOVED: Cursor lock - now cursor is free for joystick UI interaction
        // Cursor.lockState = CursorLockMode.Locked;

        // Optional: Make cursor visible (it's visible by default, but just to be explicit)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        hasJumped = false;
    }

    void Update()
    {
        // ---------------- BUILT-IN GROUND CHECK ----------------
        isGrounded = controller.isGrounded;

        // Reset jump flag when touching ground
        if (isGrounded && velocity.y <= 0)
        {
            if (hasJumped)
            {
                hasJumped = false; // Allow jumping again
                animator.SetBool("IsJumping", false);
            }

            // Reset velocity when grounded
            if (velocity.y < 0)
                velocity.y = -1f;
        }

        // ---------------- INPUT ----------------
        float x = 0f;
        float z = 0f;

        if (useJoystick && movementJoystick != null)
        {
            // Get input from joystick
            x = movementJoystick.Horizontal;
            z = movementJoystick.Vertical;
        }

        // Optional: Keyboard fallback for testing
        if (useKeyboardAsFallback && (!useJoystick || movementJoystick == null))
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        // ---------------- CAMERA RELATIVE MOVEMENT ----------------
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * z + right * x).normalized;

            // ---------------- MOVE PLAYER ----------------
            if (moveDirection.magnitude >= 0.1f)
            {
                controller.Move(moveDirection * runSpeed * Time.deltaTime);

                // Rotate toward movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Fallback if no camera transform
            Vector3 moveDirection = new Vector3(x, 0, z).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                controller.Move(moveDirection * runSpeed * Time.deltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // ---------------- JUMP ----------------
        bool jumpInput = false;

        // Check for joystick jump button (if you have a dedicated jump joystick button)
        // Using Fire1 which is typically mapped to the A button on controllers
        if (useJoystick)
        {
            jumpInput = Input.GetButtonDown("Fire1") || jumpRequested;
        }

        // Keyboard fallback for testing
        if (useKeyboardAsFallback && (!useJoystick || movementJoystick == null))
        {
            jumpInput = Input.GetButtonDown("Jump");
        }

        // Execute jump if conditions are met
        if (jumpInput && isGrounded && !hasJumped)
        {
            hasJumped = true; // Lock jump until landing
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("IsJumping", true);
            jumpRequested = false; // Reset button request
        }

        // ---------------- APPLY GRAVITY ----------------
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ---------------- ANIMATIONS ----------------
        UpdateAnimator(x, z);
    }

    // Call this method from your jump button's onClick event
    public void RequestJump()
    {
        if (isGrounded && !hasJumped)
        {
            jumpRequested = true;
        }
    }

    // Optional: Call this to switch between joystick and keyboard
    public void SetUseJoystick(bool useJoystickInput)
    {
        useJoystick = useJoystickInput;

        // Removed cursor lock state change
    }

    // Optional: Method to manually toggle cursor visibility if needed
    public void ToggleCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void UpdateAnimator(float x, float z)
    {
        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        animator.SetBool("IsRunning", isMoving);
        animator.SetBool("IsGrounded", isGrounded);
    }
}