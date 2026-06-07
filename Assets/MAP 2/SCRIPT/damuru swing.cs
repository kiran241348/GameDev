using UnityEngine;

public class DamaruSwing : MonoBehaviour
{
    public float maxAngle = 45f;
    public float speed = 2f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed) * maxAngle;
        transform.localRotation = startRotation * Quaternion.Euler(angle, 0, 0);
    }
}