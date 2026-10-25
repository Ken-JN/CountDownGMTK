using UnityEngine;
using System.Collections;

/// <summary>
/// Kontroler pemain dengan Game Feel / Juice:
/// - Coyote Time & Jump Buffering (kontrol platformer presisi)
/// - Squash & Stretch pada lompat dan mendarat
/// - Sprite Flip & Lean/Tilt saat bergerak
/// - Ghost Trail (afterimage/bayangan waktu)
/// - Efek partikel debu prosedural saat lompat & mendarat
/// - Integrasi efek suara dinamis
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Game Feel / Polish Settings")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float maxTiltAngle = 7f;
    [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.75f, 1.28f, 1f);
    [SerializeField] private Vector3 landSquashScale = new Vector3(1.25f, 0.72f, 1f);
    [SerializeField] private float squashStretchSpeed = 10f;

    [Header("Trail / Bayangan Waktu")]
    [SerializeField] private bool enableGhostTrail = true;
    [SerializeField] private float trailInterval = 0.08f;
    [SerializeField] private Color trailColor = new Color(0.4f, 0.8f, 1f, 0.45f);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float horizontalInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private Vector3 baseScale;
    private Vector3 currentScale;
    private float trailTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        currentScale = baseScale;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Ground Check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);

        // Coyote Time logic
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            // Deteksi baru mendarat (Land impact)
            if (!wasGrounded && rb.linearVelocity.y <= 0.1f)
            {
                OnLanded();
            }
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Jump Buffering logic
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Eksekusi lompat jika buffer dan coyote aktif
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            ExecuteJump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Visual Flip & Tilting
        ApplyVisualPolish();

        // Ghost Trail saat bergerak cepat
        if (enableGhostTrail && (Mathf.Abs(rb.linearVelocity.x) > 0.5f || Mathf.Abs(rb.linearVelocity.y) > 0.5f))
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f)
            {
                SpawnGhostTrail();
                trailTimer = trailInterval;
            }
        }

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Stretch visual effect
        currentScale = new Vector3(baseScale.x * jumpStretchScale.x, baseScale.y * jumpStretchScale.y, baseScale.z);

        // Sound & dust
        SoundEffectManager.Instance?.PlayJump();
        SpawnDust(6, new Color(1f, 1f, 1f, 0.6f));
    }

    private void OnLanded()
    {
        // Squash visual effect
        currentScale = new Vector3(baseScale.x * landSquashScale.x, baseScale.y * landSquashScale.y, baseScale.z);

        // Sound & dust
        SoundEffectManager.Instance?.PlayLand();
        SpawnDust(8, new Color(0.9f, 0.9f, 0.9f, 0.7f));
    }

    private void ApplyVisualPolish()
    {
        // Kembalikan scale secara halus ke baseScale
        currentScale = Vector3.Lerp(currentScale, baseScale, Time.deltaTime * squashStretchSpeed);
        transform.localScale = currentScale;

        // Facing flip
        if (horizontalInput > 0.05f)
        {
            if (sr != null) sr.flipX = false;
        }
        else if (horizontalInput < -0.05f)
        {
            if (sr != null) sr.flipX = true;
        }

        // Dynamic Lean / Tilt saat berjalan
        float targetTilt = -horizontalInput * maxTiltAngle;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 12f);
    }

    private void SpawnGhostTrail()
    {
        if (sr == null || sr.sprite == null) return;

        GameObject ghost = new GameObject("GhostTrail");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.flipX = sr.flipX;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder - 1;
        ghostSr.color = trailColor;

        StartCoroutine(FadeAndDestroyTrail(ghost, ghostSr));
    }

    private IEnumerator FadeAndDestroyTrail(GameObject ghost, SpriteRenderer ghostSr)
    {
        float duration = 0.28f;
        float elapsed = 0f;
        Color initialColor = ghostSr.color;

        while (elapsed < duration)
        {
            if (ghostSr == null) yield break;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsed / duration);
            ghostSr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (ghost != null) Destroy(ghost);
    }

    private void SpawnDust(int count, Color dustColor)
    {
        Vector3 spawnPos = groundCheck != null ? groundCheck.position : transform.position - new Vector3(0, 0.5f, 0);

        for (int i = 0; i < count; i++)
        {
            GameObject dust = new GameObject("DustParticle");
            dust.transform.position = spawnPos + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0f, 0.1f), 0f);

            SpriteRenderer dustSr = dust.AddComponent<SpriteRenderer>();
            dustSr.sprite = sr != null ? sr.sprite : null; // gunakan sprite yang sama atau kotak
            dustSr.color = dustColor;
            dustSr.sortingOrder = 5;

            float size = Random.Range(0.08f, 0.16f);
            dust.transform.localScale = Vector3.one * size;

            Vector2 velocity = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(0.8f, 2.2f));
            StartCoroutine(AnimateDust(dust, dustSr, velocity));
        }
    }

    private IEnumerator AnimateDust(GameObject dust, SpriteRenderer dustSr, Vector2 velocity)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 initialScale = dust.transform.localScale;
        Color col = dustSr.color;

        while (elapsed < duration)
        {
            if (dust == null) yield break;
            dust.transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity += Vector2.down * 6f * Time.deltaTime;

            float t = elapsed / duration;
            dust.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            dustSr.color = new Color(col.r, col.g, col.b, Mathf.Lerp(col.a, 0f, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (dust != null) Destroy(dust);
    }

    private void OnDisable()
    {
        // Reset rotasi dan scale saat membeku menjadi patung clone
        transform.rotation = Quaternion.identity;
        transform.localScale = baseScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
