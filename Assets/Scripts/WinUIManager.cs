using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Menampilkan panel kemenangan dengan animasi pop-in yang memuaskan dan
/// animasi reveal bintang satu per satu.
/// </summary>
public class WinUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Win Panel UI")]
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text summaryText;

    [Header("Star Thresholds")]
    [SerializeField] private int threeStarMax = 2;
    [SerializeField] private int twoStarMax = 4;

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelWon += HandleLevelWon;
            GameManager.Instance.OnOutOfClones += HandleOutOfClones;
        }
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

        if (summaryText != null)
            summaryText.text = $"Selesai dengan {cloneCount} clone!";

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            StartCoroutine(AnimateWinPanel(stars));
        }
    }

    private IEnumerator AnimateWinPanel(int stars)
    {
        Transform panelTransform = winPanel.transform;
        Vector3 initialScale = panelTransform.localScale;
        panelTransform.localScale = Vector3.zero;

        // Animasi Pop Bounce (unscaledDeltaTime agar berjalan mulus)
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Elastic pop formula
            float scaleMultiplier = Mathf.Sin(t * Mathf.PI * 0.7f) * 1.1f;
            if (t > 0.8f) scaleMultiplier = Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f);

            panelTransform.localScale = initialScale * scaleMultiplier;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        panelTransform.localScale = initialScale;

        // Reveal bintang berurutan
        if (starsText != null)
        {
            starsText.text = "";
            for (int i = 1; i <= 3; i++)
            {
                yield return new WaitForSecondsRealtime(0.18f);
                string currentStars = "";
                for (int s = 1; s <= 3; s++)
                {
                    if (s <= i && s <= stars)
                        currentStars += "\u2605 "; // ★
                    else
                        currentStars += "\u2606 "; // ☆
                }
                starsText.text = currentStars.Trim();
                CameraShake.Instance?.LightShake();
                SoundEffectManager.Instance?.PlaySwitch();
            }
        }
    }

    private void HandleOutOfClones()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private int CalculateStars(int cloneCount)
    {
        if (cloneCount <= threeStarMax) return 3;
        if (cloneCount <= twoStarMax) return 2;
        return 1;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance.ResetLevel();
    }

    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        // Contoh: UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }
}
