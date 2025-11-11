using UnityEngine;
using UnityEngine.Events;

public class Coin : MonoBehaviour
{
    [Header("Coin")]
    public int value = 1;
    [Tooltip("Optional: sound played when the coin is collected")] public AudioClip pickupSfx;
    [Tooltip("Optional: VFX prefab instantiated on pickup")] public GameObject pickupVfx;
    [Tooltip("Destroy delay to let SFX/VFX play")] public float destroyDelay = 0.0f;

    [Header("Events")] 
    public UnityEvent<int> onCollected; // Invoked with the coin value

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (other.attachedRigidbody == null) return;           // Only objects with Rigidbody2D
        if (!other.CompareTag("Player")) return;               // Require Player tag

        collected = true;

        // Fire event so a score/UI system can subscribe
        onCollected?.Invoke(value);

        // Optional feedback
        if (pickupVfx != null)
        {
            Instantiate(pickupVfx, transform.position, Quaternion.identity);
        }
        if (pickupSfx != null)
        {
            var cam = Camera.main;
            var pos = cam != null ? cam.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSfx, pos);
        }

        // Disable visuals and collider immediately
        DisableRenderersAndColliders();

        // Destroy object (optionally after a short delay)
        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
        else
            Destroy(gameObject);
    }

    private void DisableRenderersAndColliders()
    {
        var colliders = GetComponentsInChildren<Collider2D>(includeInactive: false);
        foreach (var c in colliders) c.enabled = false;

        var renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        foreach (var r in renderers) r.enabled = false;
    }
}

