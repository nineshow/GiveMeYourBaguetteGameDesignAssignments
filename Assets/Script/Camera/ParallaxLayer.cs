using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Parallax Settings")]
    [Range(-1f, 1f)]
    [SerializeField] private float parallaxFactor = 0.5f;

    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraMovement = cameraTransform.position - lastCameraPosition;

        transform.position += new Vector3(
            cameraMovement.x * parallaxFactor,
            //cameraMovement.y * parallaxFactor,
            0f,0f
        );

        lastCameraPosition = cameraTransform.position;
    }
}