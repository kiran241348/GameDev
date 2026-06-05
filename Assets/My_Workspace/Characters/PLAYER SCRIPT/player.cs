using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;

    // Joystick reference
    public VariableJoystick movementJoystick;

    public bool useJoystick = true;
    public bool useKeyboardAsFallback = true;

    [Header("Movement")]
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Jump Button")]
    public Button jumpButton;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip fallSound;
    public AudioClip hitObjectSound;

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
    public float walkFootstepInterval = 0.5f;
    public bool useRandomPitch = true;
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;

    [Header("Falling Settings")]
    public float fallThreshold = -5f;
    public float fallSoundDelay = 0.5f;
    public bool resetFallOnGround = true;

    [Header("Jump & Fall Transition")]
    public float jumpBufferTime = 0.5f;

    [Header("Respawn Settings")]
    public float respawnAnimationTime = 0.5f;

    [Header("Debug")]
    public bool enableDebugLogs = false;

    private Vector3 velocity;
    private bool isGrounded;
    private bool hasJumped;
    private bool jumpRequested;

    private float footstepTimer = 0f;
    private bool wasMoving = false;
    private bool wasGrounded = true;
    private bool isMoving = false;
    private bool isWalking = false;
    private float currentMoveSpeed = 0f;

    private bool isFalling = false;
    private bool fallSoundPlayed = false;
    private float fallStartTime = 0f;
    private Coroutine fallSoundCoroutine;

    private float timeSinceJump = 999f;

    // Respawn tracking
    private bool isRespawning = false;

    // Track previous falling state to prevent continuous updates
    private bool previousFallingState = false;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(RequestJump);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        hasJumped = false;
        isFalling = false;
        previousFallingState = false;
        fallSoundPlayed = false;
        timeSinceJump = 999f;
        isRespawning = false;

        // Set initial animator state
        if (animator != null)
        {
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsReSpawning", false);
        }
    }

    void Update()
    {
        // Don't process movement while respawning
        if (isRespawning)
            return;
        if (controller == null || !controller.enabled)
            return;

        timeSinceJump += Time.deltaTime;

        bool previousGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        CheckFallingState(previousGrounded);

        if (isGrounded && velocity.y <= 0)
        {
            if (hasJumped)
            {
                hasJumped = false;
                if (animator != null)
                    animator.SetBool("IsJumping", false);
            }

            if (velocity.y < 0)
                velocity.y = -1f;
        }

        float x = 0f;
        float z = 0f;

        if (useJoystick && movementJoystick != null)
        {
            x = movementJoystick.Horizontal;
            z = movementJoystick.Vertical;
        }

        if (useKeyboardAsFallback && (!useJoystick || movementJoystick == null))
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        currentMoveSpeed = new Vector2(x, z).magnitude;
        isMoving = currentMoveSpeed > 0.1f;
        isWalking = isMoving && currentMoveSpeed <= 0.5f;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * z + right * x).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                controller.Move(moveDirection * runSpeed * Time.deltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            Vector3 moveDirection = new Vector3(x, 0, z).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                controller.Move(moveDirection * runSpeed * Time.deltaTime);
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        HandleFootstepSounds();

        bool jumpInput = false;

        if (useJoystick)
        {
            jumpInput = Input.GetButtonDown("Fire1") || jumpRequested;
        }

        if (useKeyboardAsFallback && (!useJoystick || movementJoystick == null))
        {
            jumpInput = Input.GetButtonDown("Jump");
        }

        if (jumpInput && isGrounded && !hasJumped && !isRespawning)
        {
            hasJumped = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
            }
            jumpRequested = false;
            timeSinceJump = 0f;

            PlaySound(jumpSound, jumpSoundVolume);
            if (enableDebugLogs) Debug.Log("Jump sound played");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateAnimator(x, z);

        wasMoving = isMoving;
        wasGrounded = isGrounded;
    }

    private void CheckFallingState(bool previousGrounded)
    {
        bool isRecentlyJumped = timeSinceJump < jumpBufferTime;
        bool shouldBeFalling = !isGrounded && velocity.y < fallThreshold && !isRecentlyJumped && !isRespawning;

        if (hasJumped && velocity.y > 0)
        {
            shouldBeFalling = false;
        }

        // Only update falling state if it actually changed
        if (shouldBeFalling && !isFalling)
        {
            SetFallingState(true);
            fallStartTime = Time.time;

            if (fallSoundCoroutine != null)
                StopCoroutine(fallSoundCoroutine);
            fallSoundCoroutine = StartCoroutine(PlayFallSoundWithDelay());

            if (enableDebugLogs) Debug.Log($"Falling started! Velocity: {velocity.y}");
        }
        else if (!shouldBeFalling && isFalling && resetFallOnGround)
        {
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
        if (isRespawning) return;

        // Only update if state actually changed
        if (isFalling == falling)
            return;

        isFalling = falling;

        if (animator != null)
        {
            // Only set the animator parameter when state changes
            animator.SetBool("IsFalling", isFalling);
            if (enableDebugLogs) Debug.Log($"IsFalling animation set to: {isFalling}");
        }
    }

    private IEnumerator PlayFallSoundWithDelay()
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
        if (isRespawning) return;

        if (isGrounded && isWalking && !isFalling)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= walkFootstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            if (footstepTimer > 0)
            {
                footstepTimer = 0f;
            }
        }
    }

    private void PlayFootstepSound()
    {
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

    public void RequestJump()
    {
        if (isGrounded && !hasJumped && !isRespawning)
        {
            jumpRequested = true;
        }
    }

    public void OnHitObject()
    {
        if (isRespawning) return;

        PlaySound(hitObjectSound, hitObjectSoundVolume);

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        SetFallingState(true);

        if (enableDebugLogs) Debug.Log("Player hit an object!");
    }

    public void OnKickedBack()
    {
        if (isRespawning) return;

        PlaySound(hitObjectSound, hitObjectSoundVolume);
        SetFallingState(true);

        if (enableDebugLogs) Debug.Log("Player kicked back!");
    }

    public void SetUseJoystick(bool useJoystickInput)
    {
        useJoystick = useJoystickInput;
    }

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

    public void SetFootstepInterval(float interval)
    {
        walkFootstepInterval = interval;
    }

    public void SetRandomPitch(bool enabled)
    {
        useRandomPitch = enabled;
    }

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

    public bool IsFalling()
    {
        return isFalling;
    }

    public void TriggerFall()
    {
        if (isRespawning) return;
        SetFallingState(true);
        PlaySound(fallSound, fallSoundVolume);
    }

    // ============ RESPAWN METHODS ============

    // Complete reset of all states including animator
    public void ResetPlayerState()
    {
        // Stop respawning flag
        isRespawning = false;

        // Reset movement state
        velocity = Vector3.zero;

        // Reset jump and falling states
        hasJumped = false;
        isFalling = false;
        previousFallingState = false;
        fallSoundPlayed = false;
        timeSinceJump = 999f;
        jumpRequested = false;

        // Reset timers
        footstepTimer = 0f;
        fallStartTime = 0f;

        // Stop any coroutines
        if (fallSoundCoroutine != null)
        {
            StopCoroutine(fallSoundCoroutine);
            fallSoundCoroutine = null;
        }

        // Reset animator completely with ALL parameters
        if (animator != null)
        {
            // Reset all animation states
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsReSpawning", false);

            // Reset any triggers
            animator.ResetTrigger("Hit");

            // Force animator to update
            animator.Update(0f);

            if (enableDebugLogs) Debug.Log("Player animator reset successfully");
        }

        // Ensure gravity is applied correctly
        if (controller != null && controller.isGrounded)
        {
            velocity.y = -1f;
        }

        if (enableDebugLogs) Debug.Log("Player state reset on respawn");
    }

    // Call this when respawning - plays spawn animation
    public void RespawnPlayer(Vector3 respawnPosition)
    {
        StartCoroutine(RespawnCoroutine(respawnPosition));
    }

    private IEnumerator RespawnCoroutine(Vector3 respawnPosition)
    {
        // Set respawning flag to block movement
        isRespawning = true;

        // Disable character controller temporarily
        if (controller != null)
            controller.enabled = false;

        // Reset velocity
        velocity = Vector3.zero;

        // Reset all animation states BEFORE playing spawn
        if (animator != null)
        {
            // Clear all states
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsGrounded", true);

            // Trigger respawn animation
            animator.SetBool("IsReSpawning", true);

            if (enableDebugLogs) Debug.Log("Spawn animation triggered");
        }

        // Set position
        transform.position = respawnPosition;

        // Wait for spawn animation to play
        yield return new WaitForSeconds(respawnAnimationTime);

        // Turn off respawn animation
        if (animator != null)
        {
            animator.SetBool("IsReSpawning", false);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsFalling", false);

            if (enableDebugLogs) Debug.Log("Spawn animation ended");
        }

        // Re-enable character controller
        if (controller != null)
            controller.enabled = true;

        // Reset all states
        hasJumped = false;
        isFalling = false;
        previousFallingState = false;
        fallSoundPlayed = false;
        timeSinceJump = 999f;
        jumpRequested = false;
        footstepTimer = 0f;

        // Stop any playing sounds
        if (fallSoundCoroutine != null)
        {
            StopCoroutine(fallSoundCoroutine);
            fallSoundCoroutine = null;
        }

        // Ensure grounded state
        yield return new WaitForEndOfFrame();

        if (controller != null)
        {
            isGrounded = controller.isGrounded;
            if (isGrounded)
            {
                velocity.y = -1f;
            }
        }

        if (animator != null)
        {
            animator.Play("idle", 0, 0f);
            animator.Update(0f);
        }

        // Allow movement again
        isRespawning = false;

        if (enableDebugLogs) Debug.Log($"Player respawned at {respawnPosition}");
    }

    // Alternative: Quick respawn without animation (just resets state)
    public void QuickRespawn(Vector3 respawnPosition)
    {
        if (controller != null)
            controller.enabled = false;

        transform.position = respawnPosition;

        if (controller != null)
            controller.enabled = true;

        ResetPlayerState();

        if (enableDebugLogs) Debug.Log($"Quick respawn at {respawnPosition}");
    }

    void UpdateAnimator(float x, float z)
    {
        if (isRespawning) return;

        bool isMovingAnim = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        // Only update if values changed to prevent unnecessary animator calls
        if (animator.GetBool("IsRunning") != isMovingAnim)
            animator.SetBool("IsRunning", isMovingAnim);

        if (animator.GetBool("IsGrounded") != isGrounded)
            animator.SetBool("IsGrounded", isGrounded);

        // Priority system for animations - only update when states change
        if (hasJumped && !isGrounded && velocity.y > 0)
        {
            if (!animator.GetBool("IsJumping"))
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
            }
        }
        else if (isFalling)
        {
            if (animator.GetBool("IsJumping") || !animator.GetBool("IsFalling"))
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", true);
            }
        }
        else if (isGrounded)
        {
            if (animator.GetBool("IsJumping") || animator.GetBool("IsFalling"))
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", false);
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isRespawning) return;

        if (hit.gameObject.CompareTag("Obstacle") || hit.gameObject.CompareTag("Enemy"))
        {
            OnHitObject();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return;

        if (other.CompareTag("Knockback") || other.CompareTag("Hazard"))
        {
            OnKickedBack();
        }
    }
}