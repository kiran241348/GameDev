using UnityEngine;
using System.Collections;

public class PendulumSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 45f;        // Maximum swing angle in degrees
    public float swingSpeed = 2f;         // Speed of swinging
    public float startDelay = 0f;         // Delay before starting swing

    [Header("Axis Settings")]
    public bool swingX = false;           // Swing on X axis
    public bool swingY = false;           // Swing on Y axis
    public bool swingZ = true;            // Swing on Z axis (default for pendulum)

    [Header("Optional")]
    public bool autoStart = true;         // Start swinging automatically
    public bool useSmoothCurve = true;    // Use smooth sine wave movement

    private Quaternion startRotation;
    private Quaternion targetRotationLeft;
    private Quaternion targetRotationRight;
    private float timer = 0f;
    private bool isSwinging = false;

    void Start()
    {
        startRotation = transform.localRotation;

        Debug.Log($"Pendulum Start - Initial Rotation: {startRotation.eulerAngles}");

        // Calculate target rotations
        if (swingX)
        {
            targetRotationLeft = startRotation * Quaternion.Euler(-swingAngle, 0, 0);
            targetRotationRight = startRotation * Quaternion.Euler(swingAngle, 0, 0);
            Debug.Log($"Swinging on X axis: {swingAngle} degrees");
        }
        else if (swingY)
        {
            targetRotationLeft = startRotation * Quaternion.Euler(0, -swingAngle, 0);
            targetRotationRight = startRotation * Quaternion.Euler(0, swingAngle, 0);
            Debug.Log($"Swinging on Y axis: {swingAngle} degrees");
        }
        else if (swingZ)
        {
            targetRotationLeft = startRotation * Quaternion.Euler(0, 0, -swingAngle);
            targetRotationRight = startRotation * Quaternion.Euler(0, 0, swingAngle);
            Debug.Log($"Swinging on Z axis: {swingAngle} degrees");
        }

        if (autoStart)
        {
            StartSwinging();
        }
    }

    void Update()
    {
        if (!isSwinging) return;

        timer += Time.deltaTime * swingSpeed;

        if (useSmoothCurve)
        {
            // Smooth sine wave oscillation
            float t = (Mathf.Sin(timer) + 1f) / 2f; // Convert -1..1 to 0..1
            transform.localRotation = Quaternion.Lerp(targetRotationLeft, targetRotationRight, t);
        }
        else
        {
            // Linear back and forth
            float t = Mathf.PingPong(timer, 1f);
            transform.localRotation = Quaternion.Lerp(targetRotationLeft, targetRotationRight, t);
        }

        // Debug to check if swinging
        // Debug.Log($"Swinging: {isSwinging}, Timer: {timer}, Rotation: {transform.localRotation.eulerAngles}");
    }

    public void StartSwinging()
    {
        StartCoroutine(DelayedStart());
    }

    System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(startDelay);
        isSwinging = true;
        timer = 0f;
        Debug.Log("Pendulum started swinging!");
    }

    public void StopSwinging()
    {
        isSwinging = false;
        transform.localRotation = startRotation;
        Debug.Log("Pendulum stopped swinging");
    }

    public void ResetSwing()
    {
        timer = 0f;
        transform.localRotation = startRotation;
    }
}