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

    // Coyote time & jump buffer (para input más permisivo)
    [Header("Timing Aids")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // Double jump
    [Header("Double Jump")]
    public int extraJumps = 1;
    private int extraJumpsLeft;

    // Ground check
    [Header("Ground Check")] 
    public Transform groundCheck;           // Assign the GroundCheck child here
    public LayerMask groundLayer;           // Assign the Ground layer here
    public float groundCheckRadius = 0.2f;  // Tweak to fit your collider
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumpsLeft = extraJumps;
    }

    void Update()
    {
        // Ground detection using an overlap circle at the feet
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Coyote time + reset de doble salto al pisar suelo
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            extraJumpsLeft = extraJumps;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer: guarda el input de salto por un breve tiempo
        if (JumpPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Lógica de salto combinada (prioriza salto en suelo/coyote; si no, usa doble salto)
        if (jumpBufferCounter > 0f)
        {
            float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            float initialVelY = Mathf.Sqrt(2f * g * Mathf.Max(0f, jumpHeight));

            if (coyoteTimeCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialVelY);
                coyoteTimeCounter = 0f;   // consume coyote
                jumpBufferCounter = 0f;   // consume buffer
            }
            else if (extraJumpsLeft > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialVelY);
                extraJumpsLeft--;         // consume un salto aéreo
                jumpBufferCounter = 0f;   // consume buffer
            }
        }

        // Variable jump height: si suelta en ascenso, recorta altura
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
