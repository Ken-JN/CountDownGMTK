using UnityEngine;
using System.Collections;

/// <summary>
/// Efek getar kamera ringan. Panggil CameraShake.Instance.Shake() dari GameManager
/// setiap kali sebuah player membeku, untuk memberi feedback fisik ke pemain.
/// Tempelkan script ini di GameObject Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float duration = 0.15f;   // durasi singkat, jangan lama-lama biar tidak mengganggu
    [SerializeField] private float magnitude = 0.1f;   // seberapa jauh kamera bergeser

    private Vector3 originalPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>Memicu satu kali getaran ringan.</summary>
    public void Shake()
    {
        // Kalau sedang shake, hentikan dulu supaya tidak menumpuk dan posisi kamera tidak "melayang"
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalPos;
        }
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Semakin dekat ke akhir durasi, getaran semakin kecil (damping)
            float damper = 1f - (elapsed / duration);
            float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
            float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos; // pastikan kembali persis ke posisi awal
        shakeRoutine = null;
    }
}