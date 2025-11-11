using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    // Rigidbody 2D
    private Rigidbody2D rb;

    // Jump force
    [Header("Jump")]
    public float jumpForce = 12f;

    // Ground check
    [Header("Ground Check")] 
    public Transform groundCheck;           // Assign the GroundCheck child here
    public LayerMask groundLayer;           // Assign the Ground layer here
    public float groundCheckRadius = 0.2f;  // Tweak to fit your collider
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Ground detection using an overlap circle at the feet
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Basic jump: only when grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Gizmo to help tune the ground check in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
