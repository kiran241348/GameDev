using UnityEngine;

public class TestTile : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"<color=cyan>TestTile started on {gameObject.name}</color>");

        // Make sure collider is set up correctly
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            Debug.Log("Added BoxCollider");
        }
        col.isTrigger = false;

        // Change color to red so we can see it
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.red;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"<color=green>✅ TILE HIT BY: {collision.gameObject.name}</color>");

        // Change color to green when hit
        GetComponent<Renderer>().material.color = Color.green;
    }
}