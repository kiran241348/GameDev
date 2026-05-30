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

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip fallSound; // Sound when player is falling
    public AudioClip hitObjectSound; // Sound when hitting an object
    
    [Header("Sound Settings")]
    [Range(0f, 1f)]
    public float walkSoundVolume = 0.5f;
    [Range(0f, 1f)]
    public float jumpSoundVolume = 0.8f;
    [Range(0f, 1f)]
    public float fallSoundVolume = 0.7f;
    [Range(0f, 1f)]
    public float hitObjectSoundVolume = 0.8f;
    
    [Header("Footstep Timing")]
    public float walkFootstepInterval = 0.5f; // Time between footsteps when walking
    public bool useRandomPitch = true; // Randomize footstep pitch for variety
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;
    
    [Header("Falling Settings")]
    public float fallThreshold = -5f; // Velocity threshold to trigger falling animation
    public float fallSoundDelay = 0.5f; // Delay before playing fall sound
    public bool resetFallOnGround = true; // Reset falling state when grounded
    
    [Header("Debug")]
    public bool enableDebugLogs = false;

    private Vector3 velocity;
    private bool isGrounded;
    private bool hasJumped; // Tracks if player has jumped and hasn't landed yet
    private bool jumpRequested; // For mobile jump button
    
    // Sound tracking variables
    private float footstepTimer = 0f;
    private bool wasMoving = false;
    private bool wasGrounded = true;
    private bool isMoving = false;
    private bool isWalking = false;
    private float currentMoveSpeed = 0f;
    
    // Falling tracking variables
    private bool isFalling = false;
    private bool fallSoundPlayed = false;
    private float fallStartTime = 0f;
    private Coroutine fallSoundCoroutine;

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

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        // REMOVED: Cursor lock - now cursor is free for joystick UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        hasJumped = false;
        isFalling = false;
        fallSoundPlayed = false;
    }

    void Update()
    {
        // ---------------- BUILT-IN GROUND CHECK ----------------
        bool previousGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Check falling state
        CheckFallingState(previousGrounded);

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

        // Calculate movement magnitude
        currentMoveSpeed = new Vector2(x, z).magnitude;
        
        // Determine movement type
        isMoving = currentMoveSpeed > 0.1f;
        isWalking = isMoving && currentMoveSpeed <= 0.5f;

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

        // ---------------- HANDLE FOOTSTEP SOUNDS ----------------
        HandleFootstepSounds();

        // ---------------- JUMP ----------------
        bool jumpInput = false;

        // Check for joystick jump button (if you have a dedicated jump joystick button)
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
            
            // Play jump sound
            PlaySound(jumpSound, jumpSoundVolume);
            if (enableDebugLogs) Debug.Log("Jump sound played");
        }

        // ---------------- APPLY GRAVITY ----------------
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ---------------- ANIMATIONS ----------------
        UpdateAnimator(x, z);
        
        // Update wasMoving for next frame
        wasMoving = isMoving;
        wasGrounded = isGrounded;
    }
    
    private void CheckFallingState(bool previousGrounded)
    {
        // Check if player is falling (not grounded and moving downward)
        bool shouldBeFalling = !isGrounded && velocity.y < fallThreshold;
        
        if (shouldBeFalling && !isFalling)
        {
            // Start falling
            SetFallingState(true);
            fallStartTime = Time.time;
            
            // Play fall sound with delay
            if (fallSoundCoroutine != null)
                StopCoroutine(fallSoundCoroutine);
            fallSoundCoroutine = StartCoroutine(PlayFallSoundWithDelay());
            
            if (enableDebugLogs) Debug.Log($"Falling started! Velocity: {velocity.y}");
        }
        else if (!shouldBeFalling && isFalling && resetFallOnGround)
        {
            // Stop falling when grounded or moving upward
            SetFallingState(false);
            if (fallSoundCoroutine != null)
            {
                StopCoroutine(fallSoundCoroutine);
                fallSoundCoroutine = null;
            }
            fallSoundPlayed = false;
        }
    }
    
    private void SetFallingState(bool falling)
    {
        isFalling = falling;
        if (animator != null)
        {
            animator.SetBool("IsFalling", isFalling);
            if (enableDebugLogs) Debug.Log($"IsFalling animation set to: {isFalling}");
        }
    }
    
    private System.Collections.IEnumerator PlayFallSoundWithDelay()
    {
        fallSoundPlayed = false;
        yield return new WaitForSeconds(fallSoundDelay);
        
        if (isFalling && !fallSoundPlayed)
        {
            PlaySound(fallSound, fallSoundVolume);
            fallSoundPlayed = true;
            if (enableDebugLogs) Debug.Log("Fall sound played");
        }
    }
    
    private void HandleFootstepSounds()
    {
        // Only play footstep sounds when grounded and walking (not falling)
        if (isGrounded && isWalking && !isFalling)
        {
            // Update timer
            footstepTimer += Time.deltaTime;
            
            // Play footstep sound when timer exceeds interval
            if (footstepTimer >= walkFootstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving
            if (footstepTimer > 0)
            {
                footstepTimer = 0f;
            }
        }
    }
    
    private void PlayFootstepSound()
    {
        // Only play walk sound
        if (walkSound != null)
        {
            PlaySoundWithPitch(walkSound, walkSoundVolume);
            if (enableDebugLogs) 
                Debug.Log($"Walk sound played");
        }
    }
    
    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    private void PlaySoundWithPitch(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            if (useRandomPitch)
            {
                float originalPitch = audioSource.pitch;
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(clip, volume);
                audioSource.pitch = originalPitch;
            }
            else
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }

    // Call this method from your jump button's onClick event
    public void RequestJump()
    {
        if (isGrounded && !hasJumped)
        {
            jumpRequested = true;
        }
    }
    
    // Call this method when the player hits an object (from collision trigger)
    public void OnHitObject()
    {
        PlaySound(hitObjectSound, hitObjectSoundVolume);
        
        // Optional: Add hit animation
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Set falling state to true when hit
        SetFallingState(true);
        
        if (enableDebugLogs) Debug.Log("Player hit an object! Playing hit sound and falling animation");
    }
    
    // Call this when player is kicked/knocked back
    public void OnKickedBack()
    {
        PlaySound(hitObjectSound, hitObjectSoundVolume);
        SetFallingState(true);
        
        if (enableDebugLogs) Debug.Log("Player kicked back! Falling animation triggered");
    }

    // Optional: Call this to switch between joystick and keyboard
    public void SetUseJoystick(bool useJoystickInput)
    {
        useJoystick = useJoystickInput;
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
    
    // Public method to set footstep interval dynamically
    public void SetFootstepInterval(float interval)
    {
        walkFootstepInterval = interval;
    }
    
    // Public method to enable/disable random pitch
    public void SetRandomPitch(bool enabled)
    {
        useRandomPitch = enabled;
    }
    
    // Public method to update sound volumes
    public void SetWalkVolume(float volume)
    {
        walkSoundVolume = Mathf.Clamp01(volume);
    }
    
    public void SetJumpVolume(float volume)
    {
        jumpSoundVolume = Mathf.Clamp01(volume);
    }
    
    public void SetFallVolume(float volume)
    {
        fallSoundVolume = Mathf.Clamp01(volume);
    }
    
    // Public method to check if player is falling
    public bool IsFalling()
    {
        return isFalling;
    }
    
    // Public method to manually trigger fall (for knockback effects)
    public void TriggerFall()
    {
        SetFallingState(true);
        PlaySound(fallSound, fallSoundVolume);
    }

    void UpdateAnimator(float x, float z)
    {
        bool isMovingAnim = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        animator.SetBool("IsRunning", isMovingAnim);
        animator.SetBool("IsGrounded", isGrounded);
        // Note: IsFalling is already set in SetFallingState() method
    }
    
    // Collision detection for hitting objects
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if hit object has a specific tag
        if (hit.gameObject.CompareTag("Obstacle") || hit.gameObject.CompareTag("Enemy"))
        {
            OnHitObject();
        }
    }
    
    // Trigger detection for knockback objects
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Knockback") || other.CompareTag("Hazard"))
        {
            OnKickedBack();
        }
    }
}