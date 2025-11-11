using UnityEngine;

// Bridges gameplay values (velocity/ground) to Animator parameters
// Expected Animator parameters:
//  - Float "Speed"      : horizontal speed magnitude
//  - Bool  "IsGrounded" : true when on ground
//  - Float "YVelocity"  : vertical velocity (for jump/fall)
public class PlayerAnimBridge : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public PlayerJump player;

    [Header("Tuning")]
    public float speedSmoothing = 12f;
    public float speedMultiplier = 1f;
    [Tooltip("Time to keep 'Jump' bool true after a jump event (seconds)")]
    public float jumpBoolHold = 0.1f;

    private float smoothedSpeed;
    private Rigidbody2D rb;
    private float jumpBoolTimer;
    private bool warnedNoController;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (player == null) player = GetComponent<PlayerJump>();
        rb = GetComponent<Rigidbody2D>();
        // Make sure root motion does not interfere with 2D physics
        if (animator != null) animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (animator == null) return;
        if (animator.runtimeAnimatorController == null)
        {
            if (!warnedNoController)
            {
                Debug.LogWarning("PlayerAnimBridge: Animator has no RuntimeAnimatorController assigned. Assign a controller or point the bridge to the correct Animator.");
                warnedNoController = true;
            }
            return;
        }

        float rawSpeed = player != null ? player.SpeedX : (rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed * speedMultiplier, Time.deltaTime * speedSmoothing);

        bool grounded = player != null ? player.IsGrounded : false;
        float yVel = player != null ? player.YVelocity : (rb != null ? rb.linearVelocity.y : 0f);

        // Parameters for our simple controller
        animator.SetFloat("Speed", smoothedSpeed);
        animator.SetBool("IsGrounded", grounded);
        animator.SetFloat("YVelocity", yVel);

        // Parameters for StarterAssetsThirdPerson.controller
        animator.SetFloat("MotionSpeed", 1f);
        animator.SetBool("Grounded", grounded);
        bool freeFall = !grounded && yVel < -0.1f;
        animator.SetBool("FreeFall", freeFall);

        // Jump bool: set true briefly when a jump is triggered
        if (player != null && player.ConsumeJumpStartedFlag())
        {
            jumpBoolTimer = jumpBoolHold;
        }
        if (jumpBoolTimer > 0f)
        {
            animator.SetBool("Jump", true);
            jumpBoolTimer -= Time.deltaTime;
        }
        else
        {
            animator.SetBool("Jump", false);
        }
    }
}
