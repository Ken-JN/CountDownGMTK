using UnityEngine;

/// <summary>
/// Sistem audio instan dan otomatis.
/// Jika Anda belum punya file WAV/MP3 di project, skrip ini bisa menghasilkan efek suara
/// retro / 8-bit secara dinamis (prosedural) saat runtime!
/// Anda juga bisa memasukkan AudioClip kustom sendiri di Inspector jika punya aset suara.
/// </summary>
public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager _instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        var dummy = Instance;
    }

    public static SoundEffectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundEffectManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundEffectManager");
                    _instance = go.AddComponent<SoundEffectManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Custom Audio Clips (Opsional - Kosongkan jika ingin suara prosedural)")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private AudioClip freezeClip;
    [SerializeField] private AudioClip timerTickClip;
    [SerializeField] private AudioClip switchClip;
    [SerializeField] private AudioClip winClip;

    private AudioSource audioSource;
    private AudioClip generatedJumpClip;
    private AudioClip generatedLandClip;
    private AudioClip generatedBounceClip;
    private AudioClip generatedFreezeClip;
    private AudioClip generatedTickClip;
    private AudioClip generatedSwitchClip;
    private AudioClip generatedWinClip;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        GenerateProceduralClips();
    }

    private void GenerateProceduralClips()
    {
        generatedJumpClip = CreateChirp(0.12f, 320f, 680f);
        generatedLandClip = CreateNoiseThud(0.08f, 160f, 60f);
        generatedBounceClip = CreateBoing(0.22f, 240f, 820f);
        generatedFreezeClip = CreateCrystalShatter(0.35f);
        generatedTickClip = CreateTick(0.04f, 1200f);
        generatedSwitchClip = CreateClick(0.06f, 440f);
        generatedWinClip = CreateWinFanfare();
    }

    public void PlayJump()
    {
        PlayClip(jumpClip != null ? jumpClip : generatedJumpClip, 0.7f, 0.1f);
    }

    public void PlayLand()
    {
        PlayClip(landClip != null ? landClip : generatedLandClip, 0.5f, 0.08f);
    }

    public void PlayBounce()
    {
        PlayClip(bounceClip != null ? bounceClip : generatedBounceClip, 0.85f, 0.15f);
    }

    public void PlayFreeze()
    {
        PlayClip(freezeClip != null ? freezeClip : generatedFreezeClip, 0.95f, 0.05f);
    }

    public void PlayTimerTick()
    {
        PlayClip(timerTickClip != null ? timerTickClip : generatedTickClip, 0.6f, 0.02f);
    }

    public void PlaySwitch()
    {
        PlayClip(switchClip != null ? switchClip : generatedSwitchClip, 0.7f, 0.05f);
    }

    public void PlayWin()
    {
        PlayClip(winClip != null ? winClip : generatedWinClip, 0.9f, 0f);
    }

    private void PlayClip(AudioClip clip, float volume, float pitchVariation)
    {
        if (clip == null) return;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip, volume);
    }

    #region Procedural Synthesizer Generators
    private AudioClip CreateChirp(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("JumpSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateNoiseThud(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
            float noise = Random.Range(-0.2f, 0.2f);
            float envelope = Mathf.Pow(1f - t, 2f);
            samples[i] = (sine * 0.7f + noise * 0.3f) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("LandSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBoing(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, Mathf.Sqrt(t)) + Mathf.Sin(t * 30f) * 35f;
            float envelope = 1f - t;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("BounceSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateCrystalShatter(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float s1 = Mathf.Sin(2f * Mathf.PI * 1174.66f * (i / (float)sampleRate)); // D6
            float s2 = Mathf.Sin(2f * Mathf.PI * 1760.00f * (i / (float)sampleRate)); // A6
            float s3 = Mathf.Sin(2f * Mathf.PI * 2349.32f * (i / (float)sampleRate)); // D7
            float noise = Random.Range(-0.3f, 0.3f);
            float envelope = Mathf.Exp(-5f * t);
            samples[i] = ((s1 + s2 + s3) * 0.2f + noise * 0.4f) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("FreezeSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateTick(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = 1f - t;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("TickSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateClick(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-12f * t);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("ClickSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateWinFanfare()
    {
        int sampleRate = 44100;
        float noteDuration = 0.12f;
        float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
        int totalSamples = Mathf.CeilToInt(sampleRate * noteDuration * notes.Length);
        float[] samples = new float[totalSamples];

        int samplesPerNote = Mathf.CeilToInt(sampleRate * noteDuration);
        for (int n = 0; n < notes.Length; n++)
        {
            float freq = notes[n];
            for (int i = 0; i < samplesPerNote; i++)
            {
                int globalIndex = n * samplesPerNote + i;
                if (globalIndex >= totalSamples) break;

                float t = (float)i / samplesPerNote;
                float envelope = 1f - (n == notes.Length - 1 ? t * 0.5f : t);
                samples[globalIndex] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope * 0.35f;
            }
        }

        AudioClip clip = AudioClip.Create("WinFanfare", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    #endregion
}
