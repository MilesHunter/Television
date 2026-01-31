using UnityEngine;

public class FilterController : MonoBehaviour
{
    [Header("Filter Settings")]
    public FilterEffectData filterData;
    public bool isActive = true;
    public bool canBePickedUp = true;

    [Header("Visual Components")]
    public SpriteRenderer filterSprite;
    public ParticleSystem filterParticles;

    // Properties
    public FilterEffectData FilterData => filterData;
    public bool IsActive => isActive && gameObject.activeInHierarchy;
    public bool IsBeingCarried { get; private set; }

    // Components
    private Rigidbody2D rb;
    private Collider2D col;
    private AudioSource audioSource;

    // Pickup state
    private Transform originalParent;
    private Vector3 originalScale;

    // Position tracking for real-time updates
    private Vector3 lastTrackedPosition;
    private bool positionTrackingInitialized = false;

    void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        // Store original state
        originalParent = transform.parent;
        originalScale = transform.localScale;

        // Setup components if not assigned
        SetupComponents();
    }

    void Start()
    {
        // Register with filter system
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.RegisterFilter(this);
        }

        // Apply filter data
        ApplyFilterData();

        // Initialize position tracking
        InitializePositionTracking();
    }

    void OnDestroy()
    {
        // Unregister from filter system
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.UnregisterFilter(this);
        }
    }

    void SetupComponents()
    {
        // Setup sprite renderer
        if (filterSprite == null)
        {
            filterSprite = GetComponent<SpriteRenderer>();
            if (filterSprite == null)
            {
                filterSprite = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // Setup rigidbody if not present
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configure rigidbody for filters - they should be kinematic by default
        rb.isKinematic = true;
        rb.gravityScale = 0f;

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void ApplyFilterData()
    {
        if (filterData == null) return;

        // Apply visual properties
        if (filterSprite != null)
        {
            filterSprite.color = filterData.filterColor;
        }

        // Setup particles
        if (filterParticles != null && filterData.filterParticles != null)
        {
            // Copy particle system settings
            var main = filterParticles.main;
            main.startColor = filterData.filterColor;
        }
    }

    #region Pickup System

    public bool CanBePickedUp()
    {
        return canBePickedUp && !IsBeingCarried;
    }

    public void OnPickedUp(Transform carrier)
    {
        if (!CanBePickedUp()) return;

        IsBeingCarried = true;

        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Make collider a trigger
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Set parent to carrier
        transform.SetParent(carrier);

        // Play pickup sound
        PlaySound(filterData?.pickupSound);

        // Update filter system
        NotifyFilterSystemUpdate();

        Debug.Log($"Filter {name} picked up by {carrier.name}");
    }

    public void OnDropped(Vector3 dropPosition)
    {
        if (!IsBeingCarried) return;

        IsBeingCarried = false;

        // Keep rigidbody kinematic so filter stays fixed in place
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Restore collider (but keep it as trigger since we removed collision detection)
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Reset parent
        transform.SetParent(originalParent);

        // Set position
        transform.position = dropPosition;

        // Restore scale
        transform.localScale = originalScale;

        // Update filter system
        NotifyFilterSystemUpdate();

        Debug.Log($"Filter {name} dropped at {dropPosition}");
    }

    #endregion

    #region Position Tracking

    private void InitializePositionTracking()
    {
        lastTrackedPosition = transform.position;
        positionTrackingInitialized = true;
    }

    public bool HasPositionChanged()
    {
        if (!positionTrackingInitialized)
        {
            InitializePositionTracking();
            return false;
        }

        Vector3 currentPosition = transform.position;
        float distanceThreshold = 0.01f; // 1cm threshold to avoid floating point precision issues

        return Vector3.Distance(currentPosition, lastTrackedPosition) > distanceThreshold;
    }

    public void ResetPositionTracking()
    {
        lastTrackedPosition = transform.position;
    }

    #endregion

    #region State Management

    public void SetActive(bool active)
    {
        if (isActive != active)
        {
            isActive = active;
            UpdateVisualState();
            NotifyFilterSystemUpdate();
        }
    }

    private void UpdateVisualState()
    {
        // Update sprite alpha based on active state
        if (filterSprite != null)
        {
            Color color = filterSprite.color;
            color.a = isActive ? 1f : 0.5f;
            filterSprite.color = color;
        }

        // Update particles
        if (filterParticles != null)
        {
            if (isActive && !filterParticles.isPlaying)
            {
                filterParticles.Play();
            }
            else if (!isActive && filterParticles.isPlaying)
            {
                filterParticles.Stop();
            }
        }
    }

    private void NotifyFilterSystemUpdate()
    {
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.UpdateFilterEffects();
        }
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        if (filterData != null)
        {
            // Draw filter effect radius
            Gizmos.color = IsActive ? filterData.filterColor : Color.gray;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
            Gizmos.DrawSphere(transform.position, filterData.filterRadius);

            // Draw wire sphere for boundary
            Gizmos.color = IsActive ? filterData.filterColor : Color.gray;
            Gizmos.DrawWireSphere(transform.position, filterData.filterRadius);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (filterData != null)
        {
            // Draw more detailed info when selected
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, filterData.filterRadius);

            // Draw filter type info
            UnityEditor.Handles.Label(transform.position + Vector3.up * (filterData.filterRadius + 0.5f),
                $"Filter: {filterData.filterType}\nActive: {IsActive}\nCarried: {IsBeingCarried}");
        }
    }

    #endregion
}