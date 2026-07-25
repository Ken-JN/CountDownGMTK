using UnityEngine;
using System.Collections;

/// <summary>
/// Pijakan yang melontarkan player ke arah tertentu (biasanya ke atas) dengan kekuatan tetap.
/// Cocok untuk melontarkan player ke ledge tinggi secara instan, sebagai alternatif dari
/// menumpuk clone -- memberi pilihan strategi ke pemain.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BouncePad : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private Vector2 bounceDirection = Vector2.up; // arah lontar, akan dinormalisasi otomatis
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private bool overrideExistingVelocity = true; // true = reset kecepatan lama dulu sebelum lontar

    [Header("Juice / Feedback")]
    [SerializeField] private GameObject bounceParticlePrefab; // opsional, efek partikel saat terpantul
    [SerializeField] private float squashScale = 0.7f;        // seberapa gepeng pad saat kena tumbukan
    [SerializeField] private float squashDuration = 0.15f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        bounceDirection = bounceDirection.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hanya pantulkan player yang masih hidup, bukan clone yang sudah beku
        // (clone bersifat Kinematic sehingga tidak akan terpengaruh gaya fisika sama sekali)
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (overrideExistingVelocity)
            rb.linearVelocity = Vector2.zero;

        rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);

        PlayBounceFeedback();
    }

    private void PlayBounceFeedback()
    {
        // Efek visual "squash" ringan supaya pad terasa hidup, mirip pegas pinball
        StopAllCoroutines();
        StartCoroutine(SquashRoutine());

        if (bounceParticlePrefab != null)
        {
            GameObject vfx = Instantiate(bounceParticlePrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        // Screen shake super ringan opsional, konsisten dengan efek freeze sebelumnya
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    private IEnumerator SquashRoutine()
    {
        transform.localScale = new Vector3(originalScale.x * 1.15f, originalScale.y * squashScale, originalScale.z);

        float elapsed = 0f;
        while (elapsed < squashDuration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsed / squashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}