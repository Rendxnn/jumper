using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    // Jump & Physics
    private Rigidbody2D rb;
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    // Ground Check
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    // Coyote Time
    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    // Jump Buffering
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    // Double Jump
    public int extraJumps = 1;
    private int extraJumpsValue;

    // Flag to notify animation of a jump event this frame
    private bool _jumpStartedThisFrame;

    // New Input System
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput; // Assign asset with a "Player" map containing actions
    private InputAction jumpAction;                   // Button
    private InputAction moveAction;                   // Vector2 or 1D Axis

    // Horizontal Move
    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;    // Units per second
    private float moveInputX = 0f;

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
            jumpAction = playerInput.actions.FindAction("Jump", throwIfNotFound: false);
            moveAction  = playerInput.actions.FindAction("Move", throwIfNotFound: false);
        }
        extraJumpsValue = extraJumps; // Set initial jumps
    }

    void OnDisable()
    {
        jumpAction = null;
        moveAction = null;
    }

    void Update()
    {
        // --- Read horizontal input (supports Vector2 or 1D Axis) ---
        if (moveAction != null)
        {
            var ect = moveAction.expectedControlType;
            if (!string.IsNullOrEmpty(ect) && ect.ToLowerInvariant() == "axis")
            {
                moveInputX = moveAction.ReadValue<float>();
            }
            else
            {
                Vector2 mv = moveAction.ReadValue<Vector2>();
                moveInputX = mv.x;
            }
        }

        // --- Ground Check ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Coyote Time & Double Jump Reset ---
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            extraJumpsValue = extraJumps; // Reset double jumps
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // --- Jump Buffering ---
        if (JumpPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // --- COMBINED Jump Input Check ---
        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f) // Priority 1: Ground Jump (uses coyote time)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                coyoteTimeCounter = 0f; // Consume coyote time
                jumpBufferCounter = 0f; // Consume buffer
                _jumpStartedThisFrame = true;
            }
            else if (extraJumpsValue > 0) // Priority 2: Air Jump
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // You could use a different jumpForce here
                extraJumpsValue--; // Consume an air jump
                jumpBufferCounter = 0f; // Consume buffer
                _jumpStartedThisFrame = true;
            }
        }
    }

    void FixedUpdate()
    {
        // --- Apply horizontal velocity ---
        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);

        // --- Better Falling Logic ---
        if (rb.linearVelocity.y < 0)
        {
            // We are falling - apply the fallMultiplier
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !JumpHeld())
        {
            // We are rising, but not holding Jump - apply the lowJumpMultiplier
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // Helper function to visualize the ground check radius in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // --- Input helpers (New Input System) ---
    private bool JumpPressedThisFrame()
    {
        return jumpAction != null && jumpAction.WasPressedThisFrame();
    }

    private bool JumpHeld()
    {
        return jumpAction != null && jumpAction.IsPressed();
    }

    // --- Read-only accessors for Animator bridge ---
    public bool IsGrounded => isGrounded;
    public float YVelocity => rb != null ? rb.linearVelocity.y : 0f;
    public float SpeedX => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;

    // Returns true once when a jump was applied, then resets the flag
    public bool ConsumeJumpStartedFlag()
    {
        if (_jumpStartedThisFrame)
        {
            _jumpStartedThisFrame = false;
            return true;
        }
        return false;
    }
}
