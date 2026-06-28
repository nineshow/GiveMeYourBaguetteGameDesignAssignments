using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Map Sprite")]
    [SerializeField] private SpriteRenderer mapSprite;

    private Vector3 bottomLeftLimit;
    private Vector3 topRightLimit;

    private float halfHeight;
    private float halfWidth;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("CameraFollow: No Player found.");
        }

        if (mapSprite == null)
        {
            Debug.LogWarning("CameraFollow: No map sprite assigned.");
            return;
        }

        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;

        Bounds mapBounds = mapSprite.bounds;

        bottomLeftLimit = mapBounds.min + new Vector3(halfWidth, halfHeight, 0f);
        topRightLimit = mapBounds.max + new Vector3(-halfWidth, -halfHeight, 0f);
    }

    private void LateUpdate()
    {
        if (target == null || mapSprite == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        float clampedX = Mathf.Clamp(
            desiredPosition.x,
            bottomLeftLimit.x,
            topRightLimit.x
        );

        float clampedY = Mathf.Clamp(
            desiredPosition.y,
            bottomLeftLimit.y,
            topRightLimit.y
        );

        Vector3 clampedPosition = new Vector3(
            clampedX,
            clampedY,
            offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            clampedPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}