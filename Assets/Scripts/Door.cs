using UnityEngine;

/// <summary>
/// Gerbang sederhana: aktif/nonaktif collider dan sprite berdasarkan status switch.
/// </summary>
public class Door : MonoBehaviour
{
    private Collider2D doorCollider;
    private SpriteRenderer sr;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetOpen(bool open)
    {
        doorCollider.enabled = !open; // kalau terbuka, collider dimatikan supaya bisa dilewati
        sr.color = open ? new Color(1f, 1f, 1f, 0.3f) : Color.white; // transparan saat terbuka
    }
}