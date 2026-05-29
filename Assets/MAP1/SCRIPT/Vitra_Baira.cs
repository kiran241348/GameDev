using UnityEngine;
using UnityEngine.UIElements;

public class Vitra_Baira : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame

    public float speed = 0.7f;

    public float leftLimit = -23.3f;
    public float rightLimit = -21.1f;

    private bool movingRight = true;

    void Update()
    {

        // Move right
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;

            // Check if reached right side
            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
            }
        }
        // Move left
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;

            // Check if reached left side
            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
            }
        }
    
}
}
