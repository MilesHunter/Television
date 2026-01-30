using UnityEngine;

public class RevealableObject : MonoBehaviour
{
    [Header("Reveal Requirements")]
    [SerializeField] private FilterType requiredFilters = FilterType.Red;
    [SerializeField] private bool requireAllFilters = true; // true = AND logic, false = OR logic

    [Header("Visual Settings")]
    [SerializeField] private float revealTransitionSpeed = 5f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Collision Settings")]
    [SerializeField] private bool disableCollisionWhenHidden = true;
    [SerializeField] private LayerMask hiddenLayer = 0;
    [SerializeField] private LayerMask revealedLayer = 0;

    // State
    private bool isRevealed = false;
    private bool isTransitioning = false;
    private float currentRevealAmount = 0f;
    private float targetRevealAmount = 0f;

    // Components
    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;
    private Renderer[] renderers;

    // Original values
    private Color[] originalColors;
    private int originalLayer;

    // Events
    public System.Action<bool> OnRevealStateChanged;

    // Properties
    public bool IsRevealed => isRevealed;
    public float RevealAmount => currentRevealAmount;
    public FilterType RequiredFilters => requiredFilters;

    void Awake()
    {
        // Cache components
        CacheComponents();

        // Store original values
        StoreOriginalValues();
    }

    void Start()
    {
        // Register with filter system
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.RegisterRevealableObject(this);
        }

        // Set initial state
        SetInitialState();
    }

    void OnDestroy()
    {
        // Unregister from filter system
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.UnregisterRevealableObject(this);
        }
    }

    void Update()
    {
        // Handle reveal transition
        if (isTransitioning)
        {
            UpdateRevealTransition();
        }
    }

    void CacheComponents()
    {
        // Get all sprite renderers (including children)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Get all colliders (including children)
        colliders = GetComponentsInChildren<Collider2D>();

        // Get all renderers (including children)
        renderers = GetComponentsInChildren<Renderer>();
    }

    void StoreOriginalValues()
    {
        // Store original colors
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // Store original layer
        originalLayer = gameObject.layer;
    }

    void SetInitialState()
    {
        // Start hidden
        currentRevealAmount = 0f;
        targetRevealAmount = 0f;
        isRevealed = false;

        // Apply initial visual state
        ApplyVisualState(0f);
        ApplyCollisionState(false);
    }

    #region Reveal Logic

    public bool ShouldReveal(int currentFilterEffect)
    {
        int requiredValue = (int)requiredFilters;

        if (requireAllFilters)
        {
            // AND logic: all required filters must be present
            return (currentFilterEffect & requiredValue) == requiredValue;
        }
        else
        {
            // OR logic: at least one required filter must be present
            return (currentFilterEffect & requiredValue) != 0;
        }
    }

    public void SetRevealed(bool revealed)
    {
        if (isRevealed != revealed)
        {
            isRevealed = revealed;
            targetRevealAmount = revealed ? 1f : 0f;
            isTransitioning = true;

            // Invoke event
            OnRevealStateChanged?.Invoke(revealed);

            Debug.Log($"{name} reveal state changed to: {revealed}");
        }
    }

    private void UpdateRevealTransition()
    {
        // Smoothly transition to target reveal amount
        float speed = revealTransitionSpeed * Time.deltaTime;
        currentRevealAmount = Mathf.MoveTowards(currentRevealAmount, targetRevealAmount, speed);

        // Apply current state
        float curveValue = revealCurve.Evaluate(currentRevealAmount);
        ApplyVisualState(curveValue);

        // Check if transition is complete
        if (Mathf.Approximately(currentRevealAmount, targetRevealAmount))
        {
            isTransitioning = false;
            ApplyCollisionState(isRevealed);
        }
    }

    #endregion

    #region Visual State

    private void ApplyVisualState(float revealAmount)
    {
        // Update sprite renderer alpha
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color color = originalColors[i];
                color.a = originalColors[i].a * revealAmount;
                spriteRenderers[i].color = color;
            }
        }

        // Update other renderers if needed
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && !(renderer is SpriteRenderer))
            {
                Material material = renderer.material;
                Color color = material.color;
                color.a = revealAmount;
                material.color = color;
            }
        }
    }

    private void ApplyCollisionState(bool enableCollision)
    {
        if (!disableCollisionWhenHidden) return;

        // Enable/disable colliders
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = enableCollision;
            }
        }

        // Change layer if specified
        if (hiddenLayer != 0 && revealedLayer != 0)
        {
            int targetLayer = enableCollision ?
                GetLayerFromMask(revealedLayer) :
                GetLayerFromMask(hiddenLayer);

            if (targetLayer >= 0)
            {
                gameObject.layer = targetLayer;
            }
        }
    }

    private int GetLayerFromMask(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 1)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber;
    }

    #endregion

    #region Public Methods

    public void SetRequiredFilters(FilterType filters, bool requireAll = true)
    {
        requiredFilters = filters;
        requireAllFilters = requireAll;

        // Update state immediately
        if (FilterSystemManager.Instance != null)
        {
            int currentEffect = FilterSystemManager.Instance.GetFilterEffectAtPosition(transform.position);
            bool shouldReveal = ShouldReveal(currentEffect);
            SetRevealed(shouldReveal);
        }
    }

    public void ForceReveal(bool reveal)
    {
        SetRevealed(reveal);
    }

    public void SetRevealTransitionSpeed(float speed)
    {
        revealTransitionSpeed = speed;
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        // Draw reveal state indicator
        Gizmos.color = isRevealed ? Color.green : Color.red;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);

        Bounds bounds = GetBounds();
        Gizmos.DrawCube(bounds.center, bounds.size);
    }

    void OnDrawGizmosSelected()
    {
        // Draw detailed info when selected
        Bounds bounds = GetBounds();

        // Draw wire frame
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Draw required filters info
        Vector3 labelPos = bounds.center + Vector3.up * (bounds.size.y * 0.5f + 0.5f);

        #if UNITY_EDITOR
        string filterInfo = $"Required: {requiredFilters}\nLogic: {(requireAllFilters ? "AND" : "OR")}\nRevealed: {isRevealed}\nAmount: {currentRevealAmount:F2}";
        UnityEditor.Handles.Label(labelPos, filterInfo);
        #endif
    }

    private Bounds GetBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.one);

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    #endregion
}