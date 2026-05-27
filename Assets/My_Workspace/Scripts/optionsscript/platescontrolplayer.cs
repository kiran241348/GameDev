using UnityEngine;

public class PlayerPlateDetector : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GlassPlate plate = hit.collider.GetComponent<GlassPlate>();

        if (plate != null)
        {
            plate.TriggerPlate();
        }
    }
}