using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // Makes the canvas exactly match the camera's rotation.
        // This prevents flipping, inversion, and perspective issues.
        transform.rotation = mainCameraTransform.rotation;
    }
}