using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CountdownTimerManager timerManager;

    [Header("Frozen Clone Setup")]
    [SerializeField] private string frozenLayerName = "FrozenClone";
    [SerializeField] private string frozenTagName = "Platform";
    [SerializeField] private GameObject freezeParticlePrefab; // drag Particle System prefab di sini
    public int CloneCount => frozenClones.Count; // tambahkan property ini di GameManager

    [Header("Clone Limit")]
    [SerializeField] private int maxClones = 5; // batas jumlah clone sebelum "kehabisan nyawa"

    [Header("UI")]
    [SerializeField] private TMPro.TMP_Text cloneCounterText; // opsional: "Clone: 2 / 5"

    private readonly List<GameObject> frozenClones = new List<GameObject>();
    private GameObject currentPlayer;

    public event Action OnLevelWon;
    public event Action OnOutOfClones; // trigger UI "Game Over" / restart level

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        timerManager.OnTimerExpired += HandleTimerExpired;
        SpawnNewPlayer();
        timerManager.StartTimer();
        UpdateCloneCounterUI();
    }

    private void OnDestroy()
    {
        if (timerManager != null)
            timerManager.OnTimerExpired -= HandleTimerExpired;
    }

    private void HandleTimerExpired()
    {
        FreezeCurrentPlayer();

        // Cek apakah clone sudah habis
        if (frozenClones.Count >= maxClones)
        {
            OnOutOfClones?.Invoke(); // biar UI bisa tampilkan "Habis clone! Coba lagi"
            return; // jangan spawn player baru, loop berhenti
        }

        SpawnNewPlayer();
        timerManager.StartTimer();
    }

    private void FreezeCurrentPlayer()
    {
        if (currentPlayer == null) return;

        PlayerController controller = currentPlayer.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = currentPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        currentPlayer.tag = frozenTagName;
        SetLayerRecursively(currentPlayer, LayerMask.NameToLayer(frozenLayerName));

        SpriteRenderer sr = currentPlayer.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.6f, 0.6f, 0.9f, 1f);

        // --- Spawn particle effect freeze di posisi clone ---
        if (freezeParticlePrefab != null)
        {
            GameObject vfx = Instantiate(freezeParticlePrefab, currentPlayer.transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // auto-hapus setelah 2 detik, sesuaikan dengan durasi particle system
        }

        frozenClones.Add(currentPlayer);
        currentPlayer = null;

        UpdateCloneCounterUI();
    }

    private void SpawnNewPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        currentPlayer.tag = "Player";
        currentPlayer.layer = LayerMask.NameToLayer("Player");
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void UpdateCloneCounterUI()
    {
        if (cloneCounterText != null)
            cloneCounterText.text = $"Clone: {frozenClones.Count} / {maxClones}";
    }

    /// <summary>Dipanggil oleh ExitTrigger.cs saat player mencapai pintu keluar.</summary>
    public void WinLevel()
    {
        timerManager.PauseTimer();
        if (currentPlayer != null)
        {
            var controller = currentPlayer.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false; // kunci player biar tidak bisa gerak lagi
        }
        OnLevelWon?.Invoke();
        Debug.Log("Level Selesai!");
    }

    public void ResetLevel()
    {
        foreach (var clone in frozenClones)
            if (clone != null) Destroy(clone);
        frozenClones.Clear();

        if (currentPlayer != null) Destroy(currentPlayer);

        SpawnNewPlayer();
        timerManager.StartTimer();
        UpdateCloneCounterUI();
    }
}