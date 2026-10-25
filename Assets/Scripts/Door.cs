using UnityEngine;
using System.Collections;

/// <summary>
/// Pintu mekanik/energi yang terbuka dan tertutup dengan animasi halus (sliding & fading).
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private Color closedColor = Color.white;
    [SerializeField] private Color openColor = new Color(0.4f, 1f, 0.8f, 0.25f);

    private Collider2D doorCollider;
    private SpriteRenderer sr;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine transitionRoutine;
    private bool isOpen = false;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * slideDistance;
    }

    public void SetOpen(bool open)
    {
        if (isOpen == open) return;
        isOpen = open;

        doorCollider.enabled = !open;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(open));
    }

    private IEnumerator TransitionRoutine(bool open)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = open ? openPos : closedPos;
        Color startCol = sr != null ? sr.color : Color.white;
        Color targetCol = open ? openColor : closedColor;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            // Smooth ease out curve
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (sr != null) sr.color = Color.Lerp(startCol, targetCol, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        if (sr != null) sr.color = targetCol;
        transitionRoutine = null;
    }
}
