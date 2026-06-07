using UnityEngine;

public class LaunchPad1 : MonoBehaviour
{
    public Transform targetPoint;
    public float arcHeight = 5f;

    [Header("Effects")]
    public ParticleSystem effect;
    public AudioClip soundEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerLaunch1 launcher = other.GetComponent<PlayerLaunch1>();

        if (launcher == null || targetPoint == null)
            return;

        // Particle
        if (effect != null)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
        }

        // Sound
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        }

        Vector3 velocity = CalculateLaunchVelocity(
            other.transform.position,
            targetPoint.position,
            arcHeight
        );

        launcher.Launch(velocity);
    }

    Vector3 CalculateLaunchVelocity(
        Vector3 start,
        Vector3 target,
        float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        float displacementY = target.y - start.y;
        Vector3 displacementXZ = new Vector3(
            target.x - start.x,
            0,
            target.z - start.z
        );

        float timeUp = Mathf.Sqrt(2 * height / gravity);
        float timeDown = Mathf.Sqrt(
            2 * Mathf.Max(0.1f, height - displacementY) / gravity
        );

        float totalTime = timeUp + timeDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }
}