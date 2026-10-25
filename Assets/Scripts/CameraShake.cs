using UnityEngine;
using System.Collections;

/// <summary>
/// Efek getar kamera & hitstop dinamis.
/// Auto-menemukan atau menempel ke Main Camera secara otomatis (Singleton aman).
/// Panggil CameraShake.Instance.Shake(), LightShake(), MediumShake(), atau HitStop() dari skrip mana pun.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake _instance;
    public static CameraShake Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CameraShake>();
                if (_instance == null && Camera.main != null)
                {
                    _instance = Camera.main.gameObject.AddComponent<CameraShake>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.15f;
    [SerializeField] private float defaultMagnitude = 0.1f;

    private Vector3 originalPos;
    private Coroutine shakeRoutine;
    private Coroutine hitStopRoutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        originalPos = transform.localPosition;
    }

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>Shake standar dengan durasi dan magnitude bawaan.</summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    /// <summary>Shake ringan untuk benturan kecil / bumper.</summary>
    public void LightShake()
    {
        Shake(0.1f, 0.06f);
    }

    /// <summary>Shake sedang untuk player membeku jadi clone.</summary>
    public void MediumShake()
    {
        Shake(0.2f, 0.14f);
    }

    /// <summary>Shake kuat untuk trigger besar atau time out kritis.</summary>
    public void HeavyShake()
    {
        Shake(0.35f, 0.25f);
    }

    /// <summary>Memicu getaran kamera dengan durasi dan magnitude kustom.</summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalPos;
        }
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - (elapsed / duration);
            float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
            float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime; // Gunakan unscaledDeltaTime agar shake tetap jalan saat hit-stop
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }

    /// <summary>
    /// Efek micro freeze-frame (hitstop) untuk memberi rasa impak instan saat membeku atau melontar.
    /// </summary>
    public void HitStop(float duration = 0.04f)
    {
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
        }
        hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
        hitStopRoutine = null;
    }
}
