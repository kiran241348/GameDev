using UnityEngine;

public class Kickback : MonoBehaviour
{
    public float force = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();

            if (cc != null)
            {
                Vector3 dir = (other.transform.position - transform.position).normalized;
                dir.y = 0.3f;

                cc.Move(dir * force);
            }
        }
    }
}