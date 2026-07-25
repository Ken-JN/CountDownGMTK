using UnityEngine;
using TMPro;

/// <summary>
/// Menampilkan panel kemenangan dan menghitung rating bintang berdasarkan
/// seberapa sedikit clone yang dipakai pemain (efisiensi = skill).
/// Berlangganan ke event OnLevelWon dan OnOutOfClones dari GameManager.
/// </summary>
public class WinUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel; // panel saat kehabisan clone

    [Header("Win Panel UI")]
    [SerializeField] private TMP_Text starsText;      // contoh tampilan: "★★★☆☆"
    [SerializeField] private TMP_Text summaryText;     // contoh: "Selesai dengan 3 clone"

    [Header("Star Thresholds (jumlah clone maksimal untuk X bintang)")]
    [SerializeField] private int threeStarMax = 2; // pakai <=2 clone = 3 bintang
    [SerializeField] private int twoStarMax = 4;    // pakai <=4 clone = 2 bintang
    // Lebih dari itu = 1 bintang

    private void Start()
    {
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        GameManager.Instance.OnLevelWon += HandleLevelWon;
        GameManager.Instance.OnOutOfClones += HandleOutOfClones;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelWon -= HandleLevelWon;
            GameManager.Instance.OnOutOfClones -= HandleOutOfClones;
        }
    }

    private void HandleLevelWon()
    {
        int cloneCount = GameManager.Instance.CloneCount;
        int stars = CalculateStars(cloneCount);

        starsText.text = new string('\u2605', stars) + new string('\u2606', 3 - stars); // ★ terisi + ☆ kosong
        summaryText.text = $"Selesai dengan {cloneCount} clone";

        winPanel.SetActive(true);
        Time.timeScale = 0f; // opsional: pause game saat panel win muncul
    }

    private void HandleOutOfClones()
    {
        gameOverPanel.SetActive(true);
    }

    private int CalculateStars(int cloneCount)
    {
        if (cloneCount <= threeStarMax) return 3;
        if (cloneCount <= twoStarMax) return 2;
        return 1;
    }

    /// <summary>Dipanggil oleh tombol "Restart" di panel.</summary>
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        GameManager.Instance.ResetLevel();
    }

    /// <summary>Dipanggil oleh tombol "Next Level" — sesuaikan dengan sistem scene management Anda.</summary>
    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        // Contoh sederhana: UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }
}