using UnityEngine;
using System;
using UnityEngine.InputSystem; 

public class PlayerJump : MonoBehaviour
{
    // Rigidbody 2D
    private Rigidbody2D rb;

    // Input System
    [Header("Input")]
    [SerializeField] 
    private PlayerInput playerInput; // Assign in Inspector or auto-get
    private InputAction jumpAction;                   // Must exist as "Jump" in your Actions

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            jumpAction = playerInput.actions["Jump"]; // Reads the "Jump" action
        }
        extraJumpsLeft = extraJumps;
    }

    void OnDisable()
    {
        jumpAction = null;
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

    // Input helpers using the new Input System exclusively
    private bool JumpPressedThisFrame()
    {
        return jumpAction != null && jumpAction.WasPressedThisFrame();
    }

    private bool JumpReleasedThisFrame()
    {
        return jumpAction != null && jumpAction.WasReleasedThisFrame();
    }
}
