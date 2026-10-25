using UnityEngine;
using System.Collections;

/// <summary>
/// Switch lantai interaktif dengan feedback visual (menekan ke bawah, menyala, dan bersuara).
/// </summary>
public class PressurePlateSwitch : MonoBehaviour
{
    [SerializeField] private Door linkedDoor;
    [Header("Visual Feedback")]
    [SerializeField] private float pressDepth = 0.08f;
    [SerializeField] private float animSpeed = 12f;
    [SerializeField] private Color idleColor = new Color(0.8f, 0.4f, 0.2f, 1f);
    [SerializeField] private Color activeColor = new Color(0.2f, 1f, 0.6f, 1f);

    private int objectsOnPlate = 0;
    private SpriteRenderer sr;
    private Vector3 unpressedPos;
    private Vector3 pressedPos;
    private Vector3 targetPos;
    private Color targetColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        unpressedPos = transform.position;
        pressedPos = unpressedPos - new Vector3(0f, pressDepth, 0f);
        targetPos = unpressedPos;
        targetColor = idleColor;

        if (sr != null) sr.color = idleColor;
    }

    private void Update()
    {
        // Smoothly interpolate position and color
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * animSpeed);
        if (sr != null)
            sr.color = Color.Lerp(sr.color, targetColor, Time.deltaTime * animSpeed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Platform"))
        {
            objectsOnPlate++;
            UpdatePlateState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Platform"))
        {
            objectsOnPlate--;
            UpdatePlateState();
        }
    }

    private void UpdatePlateState()
    {
        bool isPressed = objectsOnPlate > 0;

        targetPos = isPressed ? pressedPos : unpressedPos;
        targetColor = isPressed ? activeColor : idleColor;

        if (isPressed)
        {
            SoundEffectManager.Instance?.PlaySwitch();
            CameraShake.Instance?.LightShake();
        }

        if (linkedDoor != null)
            linkedDoor.SetOpen(isPressed);
    }
}
