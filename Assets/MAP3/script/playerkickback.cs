using UnityEngine;

public class PlayerLaunch : MonoBehaviour
{
    public CharacterController controller;

    private Vector3 launchVelocity;
    private bool isLaunched;

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Update()
{
    if (!isLaunched)
        return;

    if (controller == null || !controller.enabled)
        return;

    launchVelocity += Physics.gravity * Time.deltaTime;

    controller.Move(launchVelocity * Time.deltaTime);

    if (controller.isGrounded && launchVelocity.y < 0)
    {
        isLaunched = false;
    }
}
    public void Launch(Vector3 velocity)
    {
        launchVelocity = velocity;
        isLaunched = true;
    }
}