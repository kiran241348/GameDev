using UnityEngine;

public class GlassPlate : MonoBehaviour
{
    public bool isSafe;

    public Material yellowMaterial;
    public Material greenMaterial;
    public Material redMaterial;

    public GameObject questionMark;
    public GameObject crossMark;

    public float fallDelay = 0.1f;

    private Renderer rend;
    private Rigidbody rb;
    private bool triggered = false;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        rb = GetComponent<Rigidbody>(); // can be NULL safely

        if (rend != null && yellowMaterial != null)
            rend.material = yellowMaterial;

        if (questionMark != null)
            questionMark.SetActive(true);

        if (crossMark != null)
            crossMark.SetActive(false);

        // ✅ SAFE CHECK (FIXES YOUR ERROR)
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // CALL THIS FROM PLAYER
    public void TriggerPlate()
    {
        if (triggered) return;
        triggered = true;

        if (isSafe)
        {
            SafePlate();
        }
        else
        {
            BrokenPlate();
        }
    }

    public void SafePlate()
    {
        if (rend != null && greenMaterial != null)
            rend.material = greenMaterial;

        if (questionMark != null)
            questionMark.SetActive(false);
    }

    public void BrokenPlate()
    {
        if (rend != null && redMaterial != null)
            rend.material = redMaterial;

        if (questionMark != null)
            questionMark.SetActive(false);

        if (crossMark != null)
            crossMark.SetActive(true);

        FallDown(); // immediate fall (no delay bugs)
    }

    void FallDown()
    {
        // ✅ SAFE CHECK BEFORE USING RIGIDBODY
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Destroy(gameObject, 3f);
    }
}