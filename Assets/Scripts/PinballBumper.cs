using UnityEngine;
using System.Collections;

/// <summary>
/// Bumper bulat ala pinball: memantulkan player berdasarkan sudut tabrakan (reflection),
/// bukan arah tetap. Semakin cepat player datang, semakin kuat pantulannya (dengan batas minimum
/// supaya tetap terasa "nendang" walau player datang pelan).
/// Gunakan Collider2D BUKAN trigger (biarkan Is Trigger = false) supaya deteksi arah tabrakan akurat.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PinballBumper : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 18f;
    [SerializeField] private float minBounceForce = 10f; // supaya pantulan tidak pernah terasa lemas
    [SerializeField] private float cooldown = 0.1f;       // cegah bounce berkali-kali dalam 1 frame akibat collision jitter

    [Header("Juice / Feedback")]
    [SerializeField] private GameObject bounceParticlePrefab;
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.1f;

    private float lastBounceTime = -999f;
    private SpriteRenderer sr;
    private Color originalColor;
    private Vector3 originalScale;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        if (Time.time - lastBounceTime < cooldown) return; // cegah bounce ganda

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        lastBounceTime = Time.time;

        // Arah pantul = kebalikan dari titik kontak (normal), inilah yang membuat efeknya seperti pinball asli:
        // datang dari kiri memantul ke kanan-atas, datang dari atas memantul ke bawah, dst.
        Vector2 bounceDirection = collision.GetContact(0).normal;

        // Kekuatan pantulan mengikuti kecepatan datang player, tapi tidak pernah di bawah minimum
        float incomingSpeed = rb.linearVelocity.magnitude;
        float finalForce = Mathf.Max(bounceForce, incomingSpeed * 1.2f, minBounceForce);

        rb.linearVelocity = Vector2.zero; // reset dulu supaya arah pantulan bersih, tidak tercampur momentum lama
        rb.AddForce(bounceDirection * finalForce, ForceMode2D.Impulse);

        PlayBounceFeedback();
    }

    private void PlayBounceFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(FlashAndPulseRoutine());

        if (bounceParticlePrefab != null)
        {
            GameObject vfx = Instantiate(bounceParticlePrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    private IEnumerator FlashAndPulseRoutine()
    {
        // Kedip warna + membesar sebentar, khas bumper pinball yang menyala saat kena
        if (sr != null) sr.color = flashColor;
        transform.localScale = originalScale * 1.25f;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            if (sr != null) sr.color = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
            transform.localScale = Vector3.Lerp(originalScale * 1.25f, originalScale, elapsed / flashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null) sr.color = originalColor;
        transform.localScale = originalScale;
    }
}