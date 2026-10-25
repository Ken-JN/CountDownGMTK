using UnityEngine;
using TMPro;
using System;
using static SoundEffectManager;

public class CountdownTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float maxTime = 10f;
    [SerializeField] private TMP_Text timerText;

    [Header("Speed Up Settings")]
    [SerializeField] private KeyCode speedUpKey = KeyCode.Return;
    [SerializeField] private float speedUpMultiplier = 3f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color speedUpColor = Color.cyan;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 3f;

    [Header("Juice & Feedback")]
    [SerializeField] private float pulseScaleMultiplier = 1.35f;
    [SerializeField] private float pulseSpeed = 8f;

    private float currentTime;
    private bool isRunning;
    private bool isSpeedingUp;
    private Vector3 originalTextScale = Vector3.one;
    private Vector3 currentTextScale = Vector3.one;
    private int lastSecondTicked = -1;

    public event Action OnTimerExpired;
    public event Action<bool> OnSpeedUpChanged;

    private void Awake()
    {
        if (timerText != null)
        {
            originalTextScale = timerText.transform.localScale;
            currentTextScale = originalTextScale;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        bool holdingSpeedUp = Input.GetKey(speedUpKey);
        if (holdingSpeedUp != isSpeedingUp)
        {
            isSpeedingUp = holdingSpeedUp;
            OnSpeedUpChanged?.Invoke(isSpeedingUp);
        }

        float speedMultiplier = isSpeedingUp ? speedUpMultiplier : 1f;
        currentTime -= Time.deltaTime * speedMultiplier;

        // Cek pulsa detik terakhir (3, 2, 1)
        if (currentTime <= warningThreshold && currentTime > 0f)
        {
            int currentSecond = Mathf.CeilToInt(currentTime);
            if (currentSecond != lastSecondTicked)
            {
                lastSecondTicked = currentSecond;
                PulseTimerText();
                SoundEffectManager.Instance?.PlayTimerTick();
                CameraShake.Instance?.LightShake();
            }
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            isSpeedingUp = false;
            UpdateUI();
            OnTimerExpired?.Invoke();
            return;
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        currentTime = maxTime;
        isRunning = true;
        isSpeedingUp = false;
        lastSecondTicked = Mathf.CeilToInt(maxTime) + 1;
        UpdateUI();
    }

    public void PauseTimer() => isRunning = false;

    private void PulseTimerText()
    {
        currentTextScale = originalTextScale * pulseScaleMultiplier;
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        timerText.text = currentTime.ToString("F1");

        // Scale Lerp (kembali dari efek pulse membesar)
        currentTextScale = Vector3.Lerp(currentTextScale, originalTextScale, Time.deltaTime * pulseSpeed);

        // Sedikit getar saat fast-forward (Enter)
        if (isSpeedingUp)
        {
            float jitterX = UnityEngine.Random.Range(-2f, 2f);
            float jitterY = UnityEngine.Random.Range(-2f, 2f);
            timerText.rectTransform.anchoredPosition = new Vector2(jitterX, jitterY);
        }
        else
        {
            timerText.rectTransform.anchoredPosition = Vector2.zero;
        }

        timerText.transform.localScale = currentTextScale;

        // Prioritas warna
        if (currentTime <= warningThreshold)
            timerText.color = Color.Lerp(timerText.color, warningColor, Time.deltaTime * 10f);
        else if (isSpeedingUp)
            timerText.color = Color.Lerp(timerText.color, speedUpColor, Time.deltaTime * 10f);
        else
            timerText.color = Color.Lerp(timerText.color, normalColor, Time.deltaTime * 10f);
    }
}
 