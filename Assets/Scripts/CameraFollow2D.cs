using UnityEngine;

/// <summary>
/// Kamera 2D halus (Smooth Camera Follow) dengan fitur auto-target ke player aktif,
/// deadzone, dan pembatas batas level (optional bounds).
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Level Bounds (Opsional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -10f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 10f);

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        // Cari player secara otomatis jika target belum diset atau player baru di-spawn
        if (target == null || !target.gameObject.activeInHierarchy || !target.CompareTag("Player"))
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                return;
        }

        Vector3 targetPos = target.position + offset;

        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

