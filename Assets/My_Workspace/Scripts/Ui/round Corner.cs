using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedUIImage : MonoBehaviour
{
    [Header("Corner Radius")]
    [Range(0, 100)] public float topLeft = 30f;
    [Range(0, 100)] public float topRight = 30f;
    [Range(0, 100)] public float bottomLeft = 30f;
    [Range(0, 100)] public float bottomRight = 30f;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        ApplyRoundedCorners();
    }

    void Update()
    {
        ApplyRoundedCorners();
    }

    void ApplyRoundedCorners()
    {
        if (image == null)
            return;

        Material mat = image.material;

        if (mat == null)
            return;

        mat.SetFloat("_TopLeft", topLeft);
        mat.SetFloat("_TopRight", topRight);
        mat.SetFloat("_BottomLeft", bottomLeft);
        mat.SetFloat("_BottomRight", bottomRight);
    }
}