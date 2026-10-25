using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Menambahkan atmosfer visual dan efek ambient secara otomatis ke scene:
/// - Floating Ambient Particles (debu/partikel melayang yang membuat level terasa hidup)
/// - Dynamic Vignette & Chromatic Aberration saat timer kritis atau speed-up aktif
/// - Auto-setup Global Volume jika belum ada di scene
/// </summary>
public class AtmosphereAndVFX : MonoBehaviour
{
    private static AtmosphereAndVFX _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (FindFirstObjectByType<AtmosphereAndVFX>() == null)
        {
            GameObject go = new GameObject("AtmosphereAndVFX");
            _instance = go.AddComponent<AtmosphereAndVFX>();
        }
    }

    [Header("Ambient Dust Motes")]
    [SerializeField] private bool enableAmbientDust = true;
    [SerializeField] private int dustParticleCount = 28;
    [SerializeField] private Vector2 areaSize = new Vector2(30f, 18f);
    [SerializeField] private Color dustColor = new Color(0.8f, 0.9f, 1f, 0.25f);

    [Header("Post Processing Controls")]
    [SerializeField] private bool autoConfigureVolume = true;

    private Volume volume;
    private Vignette vignette;
    private Bloom bloom;
    private ChromaticAberration chromaticAberration;

    private float baseVignetteIntensity = 0.25f;
    private float targetVignetteIntensity = 0.25f;
    private float targetChromaticIntensity = 0f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        if (enableAmbientDust)
        {
            SpawnAmbientDustField();
        }

        if (autoConfigureVolume)
        {
            SetupPostProcessing();
        }

        // Hubungkan dengan TimerManager untuk efek panik / speed-up
        CountdownTimerManager timer = FindFirstObjectByType<CountdownTimerManager>();
        if (timer != null)
        {
            timer.OnSpeedUpChanged += HandleSpeedUpChanged;
        }
    }

    private void OnDestroy()
    {
        CountdownTimerManager timer = FindFirstObjectByType<CountdownTimerManager>();
        if (timer != null)
        {
            timer.OnSpeedUpChanged -= HandleSpeedUpChanged;
        }
    }

    private void Update()
    {
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * 6f);
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, targetChromaticIntensity, Time.deltaTime * 8f);
        }
    }

    private void HandleSpeedUpChanged(bool isSpeedingUp)
    {
        if (isSpeedingUp)
        {
            targetVignetteIntensity = 0.45f;
            targetChromaticIntensity = 0.35f;
            CameraShake.Instance?.LightShake();
        }
        else
        {
            targetVignetteIntensity = baseVignetteIntensity;
            targetChromaticIntensity = 0f;
        }
    }

    private void SetupPostProcessing()
    {
        // Pastikan Camera post-processing aktif
        if (Camera.main != null)
        {
            var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (cameraData != null)
            {
                cameraData.renderPostProcessing = true;
            }
        }

        volume = FindFirstObjectByType<Volume>();
        if (volume == null)
        {
            GameObject volumeGo = new GameObject("Global PostProcess Volume");
            volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            // Bloom untuk glow neon
            bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.2f);
            bloom.threshold.Override(0.85f);

            // Vignette untuk framing fokus visual
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(baseVignetteIntensity);
            vignette.smoothness.Override(0.4f);

            // Chromatic Aberration untuk distorsi kecepatan/panik
            chromaticAberration = profile.Add<ChromaticAberration>(true);
            chromaticAberration.intensity.Override(0f);
        }
        else if (volume.profile != null)
        {
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromaticAberration);

            if (bloom != null) bloom.intensity.Override(1.2f);
            if (vignette != null) vignette.intensity.Override(baseVignetteIntensity);
        }
    }

    private void SpawnAmbientDustField()
    {
        GameObject parent = new GameObject("AmbientDustField");
        parent.transform.position = Vector3.zero;

        for (int i = 0; i < dustParticleCount; i++)
        {
            GameObject mote = new GameObject($"DustMote_{i}");
            mote.transform.SetParent(parent.transform);

            Vector3 startPos = new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                0f
            );
            mote.transform.position = startPos;

            SpriteRenderer sr = mote.AddComponent<SpriteRenderer>();
            sr.color = dustColor;
            sr.sortingOrder = -5;

            float scale = Random.Range(0.04f, 0.12f);
            mote.transform.localScale = Vector3.one * scale;

            float speedX = Random.Range(-0.3f, 0.3f);
            float speedY = Random.Range(-0.2f, 0.4f);
            StartCoroutine(FloatingMoteRoutine(mote, startPos, speedX, speedY));
        }
    }

    private System.Collections.IEnumerator FloatingMoteRoutine(GameObject mote, Vector3 origin, float speedX, float speedY)
    {
        float seed = Random.Range(0f, 100f);

        while (mote != null)
        {
            float xOffset = Mathf.Sin(Time.time * 0.8f + seed) * 1.5f + (speedX * Time.time);
            float yOffset = Mathf.Cos(Time.time * 0.6f + seed) * 1.2f + (speedY * Time.time);

            float posX = Mathf.Repeat(origin.x + xOffset + areaSize.x * 0.5f, areaSize.x) - areaSize.x * 0.5f;
            float posY = Mathf.Repeat(origin.y + yOffset + areaSize.y * 0.5f, areaSize.y) - areaSize.y * 0.5f;

            mote.transform.position = new Vector3(posX, posY, 0f);
            yield return null;
        }
    }
}

