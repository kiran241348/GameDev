using UnityEngine;
using System.Collections;

public class KnockbackPlate : MonoBehaviour
{
    public float knockbackDistance = 5f;
    public float knockbackDuration = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller =
                other.GetComponent<CharacterController>();

            if (controller != null)
            {
                StartCoroutine(KnockbackPlayer(other.transform, controller));
            }
        }
    }

    IEnumerator KnockbackPlayer(Transform player,
                                CharacterController controller)
    {
        Vector3 startPos = player.position;

        // direction away from plate
        Vector3 direction =
            (player.position - transform.position).normalized;

        Vector3 targetPos =
            startPos + direction * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 move =
                Vector3.Lerp(startPos, targetPos,
                elapsed / knockbackDuration);

            controller.enabled = false;
            player.position = move;
            controller.enabled = true;

            yield return null;
        }
    }
}