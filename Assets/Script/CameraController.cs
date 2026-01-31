using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The player or object to follow
    public string playerTag = "Player"; // Automatically find player if target not set

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 2, -10); // Offset from target
    public float followSpeed = 2f; // How fast the camera follows
    public bool useFixedUpdate = false; // Use FixedUpdate for smoother physics-based movement

    [Header("Smoothing")]
    public bool enableSmoothing = true;
    public float smoothTime = 0.3f; // Time for smooth damping

    [Header("Boundaries (Optional)")]
    public bool useBoundaries = false;
    public Vector2 minBounds = new Vector2(-10, -5);
    public Vector2 maxBounds = new Vector2(10, 5);

    [Header("Dead Zone")]
    public bool useDeadZone = false;
    public Vector2 deadZoneSize = new Vector2(2f, 1f);

    // Private variables
    private Vector3 velocity = Vector3.zero; // For SmoothDamp
    private Vector3 lastTargetPosition;
    private Camera cam;

    void Start()
    {
        // Get camera component
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController: No Camera component found!");
        }

        // Auto-find target if not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"CameraController: Auto-found target: {target.name}");
            }
            else
            {
                Debug.LogWarning($"CameraController: No target set and no GameObject with tag '{playerTag}' found!");
            }
        }

        // Initialize last target position
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void LateUpdate()
    {
        if (!useFixedUpdate)
        {
            UpdateCameraPosition();
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateCameraPosition();
        }
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // Handle dead zone
        if (useDeadZone)
        {
            Vector3 currentPos = transform.position;
            Vector3 targetDiff = target.position - (currentPos - offset);

            // Only move camera if target is outside dead zone
            if (Mathf.Abs(targetDiff.x) > deadZoneSize.x / 2f)
            {
                float moveX = targetDiff.x - (deadZoneSize.x / 2f * Mathf.Sign(targetDiff.x));
                targetPosition.x = currentPos.x + moveX;
            }
            else
            {
                targetPosition.x = currentPos.x;
            }

            if (Mathf.Abs(targetDiff.y) > deadZoneSize.y / 2f)
            {
                float moveY = targetDiff.y - (deadZoneSize.y / 2f * Mathf.Sign(targetDiff.y));
                targetPosition.y = currentPos.y + moveY;
            }
            else
            {
                targetPosition.y = currentPos.y;
            }

            // Keep Z offset
            targetPosition.z = target.position.z + offset.z;
        }

        // Apply boundaries
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        // Move camera
        if (enableSmoothing)
        {
            // Smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            // Direct movement with lerp for speed control
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }

        // Update last target position
        lastTargetPosition = target.position;
    }

    #region Public Methods

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void SetFollowSpeed(float speed)
    {
        followSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetSmoothTime(float time)
    {
        smoothTime = Mathf.Max(0.01f, time);
    }

    public void SetBoundaries(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBoundaries = true;
    }

    public void DisableBoundaries()
    {
        useBoundaries = false;
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;

            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            transform.position = targetPosition;
            velocity = Vector3.zero;
        }
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        // Draw boundaries
        if (useBoundaries)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, transform.position.z);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }

        // Draw dead zone
        if (useDeadZone && target != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 deadZoneCenter = transform.position;
            deadZoneCenter.z = target.position.z;
            Gizmos.DrawWireCube(deadZoneCenter, new Vector3(deadZoneSize.x, deadZoneSize.y, 0));
        }

        // Draw target connection
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);

            // Draw offset position
            Gizmos.color = Color.blue;
            Vector3 offsetPos = target.position + offset;
            Gizmos.DrawWireSphere(offsetPos, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw more detailed info when selected
        if (target != null)
        {
            // Draw follow range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target.position, 1f);

            #if UNITY_EDITOR
            // Draw labels
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"Camera Controller\nTarget: {target.name}\nSmooth: {enableSmoothing}\nSpeed: {followSpeed}");
            #endif
        }
    }

    #endregion
}