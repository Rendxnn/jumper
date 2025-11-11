using UnityEngine;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // New Input System
#endif

public class PlayerJump : MonoBehaviour
{
    // Rigidbody 2D
    private Rigidbody2D rb;

    // Jump parameters (parametrized by height)
    [Header("Jump")]
    [Tooltip("Altura máxima deseada del salto (en unidades de mundo)")]
    public float jumpHeight = 3f;
    [Tooltip("Factor para recortar el salto al soltar el botón (0-1). 0.5 = corta a la mitad la velocidad vertical ascendente")]
    [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.5f;

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

        // Basic jump (parametrized by height): only when grounded
        if (JumpPressedThisFrame() && isGrounded)
        {
            float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            float initialVelY = Mathf.Sqrt(2f * g * Mathf.Max(0f, jumpHeight));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialVelY);
        }

        // Variable jump height: if button released while moving up, cut upward velocity
        if (JumpReleasedThisFrame() && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }
 
    // Gizmo to help tune the ground check in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // Input helpers compatible with the new Input System
    private bool JumpPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame; // A/Cross
        return keyboard || gamepad;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private bool JumpReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasReleasedThisFrame;
        return keyboard || gamepad;
#else
        return Input.GetButtonUp("Jump");
#endif
    }
}
