using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Conditions")]
    [Tooltip("World Y threshold below which the player dies")] public float deathY = -100f;
    [Tooltip("Tag used by hazards that kill the player")] public string hazardTag = "Hazard";

    [Header("Restart")] 
    [Tooltip("Delay before reloading the current scene")]
    public float restartDelay = 0.5f;

    [Header("Feedback (optional)")]
    public AudioClip deathSfx;
    public GameObject deathVfx;

    private bool isDead;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead) return;
        if (transform.position.y < deathY)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag(hazardTag))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.collider != null && collision.collider.CompareTag(hazardTag))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Optional: feedback
        if (deathVfx != null)
        {
            Instantiate(deathVfx, transform.position, Quaternion.identity);
        }
        if (deathSfx != null)
        {
            var cam = Camera.main;
            var pos = cam != null ? cam.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(deathSfx, pos);
        }

        // Optional: stop physics so the body doesn't keep moving during delay
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Invoke(nameof(ReloadScene), Mathf.Max(0f, restartDelay));
    }

    private void ReloadScene()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}

