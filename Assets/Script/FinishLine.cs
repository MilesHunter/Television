using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [Header("Finish Line Settings")]
    public bool isActive = true;
    public string playerTag = "Player";

    [Header("Visual Effects")]
    public ParticleSystem completionParticles;
    public AudioClip completionSound;

    [Header("Animation")]
    public bool animateOnCompletion = true;
    public float animationDuration = 1f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);

    // Components
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    // State
    private bool hasBeenTriggered = false;
    private Vector3 originalScale;

    // Events
    public System.Action OnLevelCompleted;

    void Awake()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Store original scale for animation
        originalScale = transform.localScale;

        // Setup components if needed
        SetupComponents();
    }

    void SetupComponents()
    {
        // Ensure we have a collider for trigger detection
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player reached the finish line
        if (isActive && !hasBeenTriggered && other.CompareTag(playerTag))
        {
            TriggerLevelCompletion();
        }
    }

    void TriggerLevelCompletion()
    {
        if (hasBeenTriggered) return;

        hasBeenTriggered = true;

        Debug.Log("Level completed! Player reached finish line.");

        // Play completion effects
        PlayCompletionEffects();

        // Animate finish line
        if (animateOnCompletion)
        {
            StartCoroutine(AnimateCompletion());
        }

        // Notify systems about level completion
        NotifyLevelCompletion();

        // Trigger event
        OnLevelCompleted?.Invoke();
    }

    void PlayCompletionEffects()
    {
        // Play particles
        if (completionParticles != null)
        {
            completionParticles.Play();
        }

        // Play sound
        if (audioSource != null && completionSound != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
    }

    System.Collections.IEnumerator AnimateCompletion()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            float scaleMultiplier = animationCurve.Evaluate(progress);

            transform.localScale = originalScale * scaleMultiplier;

            // Optional: Change color/alpha during animation
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0.5f, progress);
                spriteRenderer.color = color;
            }

            yield return null;
        }

        // Reset scale
        transform.localScale = originalScale;
    }

    void NotifyLevelCompletion()
    {
        // Get current level index
        int currentLevel = 0;
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.CurrentLevel;
        }

        // Mark level as completed in save system
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CompleteLevel(currentLevel);
        }

        // Show victory screen after a short delay
        Invoke(nameof(ShowVictoryScreen), 1.5f);
    }

    void ShowVictoryScreen()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictoryScreen();
        }
    }

    #region Public Methods

    public void SetActive(bool active)
    {
        isActive = active;

        // Update visual state
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = active ? 1f : 0.5f;
            spriteRenderer.color = color;
        }

        // Update collider
        if (col != null)
        {
            col.enabled = active;
        }
    }

    public void ResetFinishLine()
    {
        hasBeenTriggered = false;
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        SetActive(true);
    }

    public bool HasBeenTriggered()
    {
        return hasBeenTriggered;
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        // Draw finish line area
        Gizmos.color = isActive ? Color.green : Color.gray;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

        // Get collider bounds
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawCube(transform.position, collider.bounds.size);
        }
        else
        {
            // Default size if no collider
            Gizmos.DrawCube(transform.position, Vector3.one);
        }

        // Draw wire frame
        Gizmos.color = isActive ? Color.green : Color.gray;
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw more detailed info when selected
        Gizmos.color = Color.yellow;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"Finish Line\nActive: {isActive}\nTriggered: {hasBeenTriggered}");
        #endif
    }

    #endregion
}