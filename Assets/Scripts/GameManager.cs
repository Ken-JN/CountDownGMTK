using System;
using System.Collections;
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
    [SerializeField] private Color frozenColor = new Color(0.6f, 0.75f, 1f, 1f); // warna es/kristal
    public int CloneCount => frozenClones.Count;

    [Header("Clone Limit")]
    [SerializeField] private int maxClones = 5;

    [Header("UI")]
    [SerializeField] private TMPro.TMP_Text cloneCounterText;

    private readonly List<GameObject> frozenClones = new List<GameObject>();
    private GameObject currentPlayer;

    public event Action OnLevelWon;
    public event Action OnOutOfClones;

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

        if (frozenClones.Count >= maxClones)
        {
            OnOutOfClones?.Invoke();
            return;
        }

        SpawnNewPlayer();
        timerManager.StartTimer();
    }

    private void FreezeCurrentPlayer()
    {
        if (currentPlayer == null) return;

        GameObject frozenObj = currentPlayer;
        PlayerController controller = frozenObj.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = frozenObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        frozenObj.tag = frozenTagName;
        SetLayerRecursively(frozenObj, LayerMask.NameToLayer(frozenLayerName));

        // Game feel & audio feedback
        CameraShake.Instance?.MediumShake();
        CameraShake.Instance?.HitStop(0.04f);
        SoundEffectManager.Instance?.PlayFreeze();

        // Flash & crystal animation
        StartCoroutine(FreezeFlashRoutine(frozenObj));

        // VFX Partikel
        if (freezeParticlePrefab != null)
        {
            GameObject vfx = Instantiate(freezeParticlePrefab, frozenObj.transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        else
        {
            SpawnProceduralCrystalBurst(frozenObj.transform.position);
        }

        frozenClones.Add(frozenObj);
        currentPlayer = null;

        UpdateCloneCounterUI();
    }

    private IEnumerator FreezeFlashRoutine(GameObject cloneObj)
    {
        SpriteRenderer sr = cloneObj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 originalScale = cloneObj.transform.localScale;

        // Flash putih terang & pop scale sedikit
        sr.color = Color.white;
        cloneObj.transform.localScale = originalScale * 1.2f;

        yield return new WaitForSecondsRealtime(0.08f);

        if (cloneObj == null) yield break;

        // Kembali ke warna beku & scale normal
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            if (cloneObj == null) yield break;
            sr.color = Color.Lerp(Color.white, frozenColor, elapsed / duration);
            cloneObj.transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        sr.color = frozenColor;
        cloneObj.transform.localScale = originalScale;
    }

    private void SpawnProceduralCrystalBurst(Vector3 position)
    {
        for (int i = 0; i < 14; i++)
        {
            GameObject shard = new GameObject("CrystalShard");
            shard.transform.position = position;

            SpriteRenderer shardSr = shard.AddComponent<SpriteRenderer>();
            shardSr.color = new Color(0.7f, 0.9f, 1f, 0.9f);
            shardSr.sortingOrder = 10;

            float size = UnityEngine.Random.Range(0.08f, 0.18f);
            shard.transform.localScale = Vector3.one * size;

            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = UnityEngine.Random.Range(2.5f, 6.5f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            StartCoroutine(AnimateShard(shard, shardSr, velocity));
        }
    }

    private IEnumerator AnimateShard(GameObject shard, SpriteRenderer shardSr, Vector2 velocity)
    {
        float duration = 0.45f;
        float elapsed = 0f;
        Vector3 initialScale = shard.transform.localScale;

        while (elapsed < duration)
        {
            if (shard == null) yield break;
            shard.transform.position += (Vector3)(velocity * Time.unscaledDeltaTime);
            shard.transform.Rotate(0, 0, 360f * Time.unscaledDeltaTime);
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.unscaledDeltaTime * 4f);

            float t = elapsed / duration;
            shard.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            shardSr.color = new Color(shardSr.color.r, shardSr.color.g, shardSr.color.b, 1f - t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (shard != null) Destroy(shard);
    }

    private void SpawnNewPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        currentPlayer.tag = "Player";
        currentPlayer.layer = LayerMask.NameToLayer("Player");

        // Animasi spawn pop-in halus
        StartCoroutine(SpawnPopRoutine(currentPlayer));
    }

    private IEnumerator SpawnPopRoutine(GameObject playerObj)
    {
        if (playerObj == null) yield break;
        Vector3 targetScale = playerObj.transform.localScale;
        playerObj.transform.localScale = Vector3.zero;

        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (playerObj == null) yield break;
            float t = elapsed / duration;
            // Overshoot bounce (0 -> 1.15 -> 1.0)
            float scaleMultiplier = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.15f;
            playerObj.transform.localScale = targetScale * scaleMultiplier;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (playerObj != null)
            playerObj.transform.localScale = targetScale;
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

    public void WinLevel()
    {
        timerManager.PauseTimer();
        if (currentPlayer != null)
        {
            var controller = currentPlayer.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
        }

        SoundEffectManager.Instance?.PlayWin();
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
