using UnityEngine;
using System.Collections;

/// <summary>
/// Pintu keluar/portal exit:
/// - Animasi melayang (bobbing) & rotasi/pulse lembut
/// - Animasi menghisap (suction) player saat level berhasil diselesaikan
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    [Header("Idle Floating Animation")]
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 2.5f;
    [SerializeField] private bool rotateSlowly = false;
    [SerializeField] private float rotationSpeed = 30f;

    private Vector3 basePosition;
    private bool hasTriggered = false;

    private void Start()
    {
        basePosition = transform.position;
    }

    private void Update()
    {
        if (hasTriggered) return;

        // Efek melayang halus
        float newY = basePosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(basePosition.x, newY, basePosition.z);

        if (rotateSlowly)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(PortalSuctionRoutine(other.gameObject));
        }
    }

    private IEnumerator PortalSuctionRoutine(GameObject player)
    {
        // Nonaktifkan kontrol player saat tersedot ke portal
        var controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = transform.position;
        Vector3 startScale = player.transform.localScale;

        float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            player.transform.Rotate(0, 0, 720f * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.SetActive(false);
        GameManager.Instance.WinLevel();
    }
}
