using UnityEngine;

/// <summary>
/// Tempel script ini di GameObject pintu keluar/exit yang punya Collider2D (Is Trigger = true).
/// Saat player (bukan clone!) menyentuhnya, level dianggap selesai.
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hanya trigger jika yang menyentuh adalah player yang MASIH HIDUP (tag "Player"),
        // bukan clone yang sudah membeku (tag "Platform")
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.WinLevel();
        }
    }
}