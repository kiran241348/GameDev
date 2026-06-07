using UnityEngine;

public class LaunchPad2 : MonoBehaviour
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

        PlayerLaunch2 launcher =
            other.GetComponent<PlayerLaunch2>();

        if (launcher == null || targetPoint == null)
            return;

        if (effect != null)
            Instantiate(effect, transform.position, Quaternion.identity);

        if (soundEffect != null)
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);

        Vector3 velocity = CalculateLaunchVelocity(
            other.transform.position,
            targetPoint.position,
            arcHeight
        );

        launcher.Launch(velocity);
    }

    private Vector3 CalculateLaunchVelocity(
        Vector3 start,
        Vector3 target,
        float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        float displacementY = target.y - start.y;

        Vector3 displacementXZ = new Vector3(
            target.x - start.x,
            0f,
            target.z - start.z
        );

        float timeUp = Mathf.Sqrt(2f * height / gravity);

        float timeDown = Mathf.Sqrt(
            2f * Mathf.Max(0.1f, height - displacementY) / gravity
        );

        float totalTime = timeUp + timeDown;

        Vector3 velocityY =
            Vector3.up * Mathf.Sqrt(2f * gravity * height);

        Vector3 velocityXZ =
            displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }
}