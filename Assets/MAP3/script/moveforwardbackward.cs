using UnityEngine;

public class PatrolMovement : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Transform target;

    private void Start()
    {
        target = pointB;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            target = target == pointA ? pointB : pointA;
        }
    }
}