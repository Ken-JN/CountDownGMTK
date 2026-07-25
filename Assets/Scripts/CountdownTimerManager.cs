using UnityEngine;
using TMPro;
using System;

public class CountdownTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float maxTime = 10f;
    [SerializeField] private TMP_Text timerText;

    [Header("Speed Up Settings")]
    [SerializeField] private KeyCode speedUpKey = KeyCode.Return;
    [SerializeField] private float speedUpMultiplier = 3f; // waktu berjalan 3x lebih cepat saat Enter ditahan
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color speedUpColor = Color.cyan;   // warna saat dipercepat
    [SerializeField] private Color warningColor = Color.red;    // warna saat waktu hampir habis
    [SerializeField] private float warningThreshold = 3f;       // di bawah 3 detik = warning

    private float currentTime;
    private bool isRunning;
    private bool isSpeedingUp;

    public event Action OnTimerExpired;
    public event Action<bool> OnSpeedUpChanged; // beri tahu sistem lain (misal VFX kamera) saat speed-up mulai/berhenti

    private void Update()
    {
        if (!isRunning) return;

        // Cek apakah tombol Enter sedang ditahan
        bool holdingSpeedUp = Input.GetKey(speedUpKey);
        if (holdingSpeedUp != isSpeedingUp)
        {
            isSpeedingUp = holdingSpeedUp;
            OnSpeedUpChanged?.Invoke(isSpeedingUp); // trigger efek visual di script lain
        }

        // Kurangi waktu lebih cepat kalau sedang speed-up
        float speedMultiplier = isSpeedingUp ? speedUpMultiplier : 1f;
        currentTime -= Time.deltaTime * speedMultiplier;

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
        UpdateUI();
    }

    public void PauseTimer() => isRunning = false;

    private void UpdateUI()
    {
        if (timerText == null) return;

        timerText.text = currentTime.ToString("F1");

        // Prioritas warna: warning (waktu kritis) > speed-up > normal
        if (currentTime <= warningThreshold)
            timerText.color = Color.Lerp(timerText.color, warningColor, Time.deltaTime * 10f);
        else if (isSpeedingUp)
            timerText.color = Color.Lerp(timerText.color, speedUpColor, Time.deltaTime * 10f);
        else
            timerText.color = Color.Lerp(timerText.color, normalColor, Time.deltaTime * 10f);
    }
}