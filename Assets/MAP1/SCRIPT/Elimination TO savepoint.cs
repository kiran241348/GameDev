using UnityEngine;
using System.Collections;

public class ResetPositionOnTouch : MonoBehaviour
{
    [Header("Setup")]
    public Transform savePoint; // Where player goes after elimination
    public string playerTag = "Player";

    [Header("Animation")]
    public float animationDuration = 1f;

    [Header("Effects")]
    public ParticleSystem particles;
    public AudioSource sound;

    private bool isRespawn = false;

    void Start()
    {
        // Make collider a trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRespawn) return;
        if (!other.CompareTag(playerTag)) return;

        Debug.Log("Player eliminated! Moving to save point...");

        // Play effects
        if (particles != null) particles.Play();
        if (sound != null) sound.Play();

        StartCoroutine(EliminatePlayer(other.gameObject));
    }

    IEnumerator EliminatePlayer(GameObject player)
    {
        

        // Get components
        CharacterController controller = player.GetComponent<CharacterController>();
        Animator animator = player.GetComponent<Animator>();

        // Disable controller
        if (controller != null) controller.enabled = false;

        yield return new WaitForEndOfFrame();

        // Move to save point
        if (savePoint != null)
        {
            player.transform.position = savePoint.position;
            Debug.Log($"Player moved to save point: {savePoint.position}");
            isRespawn = true;
            isRespawn = true;
        }

        // Reset velocity
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Re-enable controller
        if (controller != null) controller.enabled = true;

        // Trigger IsReSpawn animation
        if (animator != null)
        {
            animator.SetBool("IsReSpawn", true);
            Debug.Log("IsReSpawn = TRUE");

            yield return new WaitForSeconds(animationDuration);

            animator.SetBool("IsReSpawn", false);
            Debug.Log("IsReSpawn = FALSE");
        }

        isRespawn = false;
        Debug.Log("Respawn complete!");
    }
}