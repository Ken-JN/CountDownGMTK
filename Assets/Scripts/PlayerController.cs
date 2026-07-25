using UnityEngine;

/// <summary>
/// Handles horizontal movement and jumping for the currently active player.
/// This script is DISABLED (not destroyed) when a player freezes into a clone,
/// which is what stops it from reading input while leaving it in the scene.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;   // Empty child object at the player's feet
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;   // Set this to include "Ground" AND "FrozenClone"

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Read input every frame (only happens while this script is enabled)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Ground check via a small overlap circle at the player's feet
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement in FixedUpdate for consistent physics
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        // NOTE: If you're on an older Unity/Physics2D version, this might be rb.velocity instead of rb.linearVelocity.
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical velocity first
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // Optional: draw the ground check radius in the Scene view for debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}