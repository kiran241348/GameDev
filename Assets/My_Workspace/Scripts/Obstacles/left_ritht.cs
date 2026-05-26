using UnityEngine;

public class left_ritht : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float speed = 0.7f;

    public float leftLimit = -23.3f;
    public float rightLimit = -21.1f;

    private bool movingRight = true;

    // Update is called once per frame
    void Update()
    {
        // Move right
        if (movingRight)
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;

            // Check if reached right side
            if (transform.position.z >= rightLimit)
            {
                movingRight = false;
            }
        }
        // Move left
        else
        {
            transform.position += Vector3.back * speed * Time.deltaTime;

            // Check if reached left side
            if (transform.position.z <= leftLimit)
            {
                movingRight = true;
            }
        }

    }
}
