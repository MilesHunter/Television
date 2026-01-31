using UnityEngine;

public class RevealableObject : MonoBehaviour
{
    [Header("Reveal Requirements")]
    [SerializeField] private FilterType requiredFilters = FilterType.Red;
    [SerializeField] private bool requireAllFilters = true; // true = AND logic, false = OR logic

    [Header("Rendering Mode")]
    [SerializeField] private RevealRenderingMode renderingMode = RevealRenderingMode.Transparency;
    [SerializeField] private bool autoSetupMaskRenderer = true;

    [Header("Visual Settings")]
    [SerializeField] private float revealTransitionSpeed = 5f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Masking Settings")]
    [SerializeField] private int maskResolution = 256;
    [SerializeField] private bool enableMaskLOD = true;
    [SerializeField] private float maskUpdateInterval = 0.2f;

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
    private FilterMaskRenderer maskRenderer;

    // Original values
    private Color[] originalColors;
    private int originalLayer;

    // Events
    public System.Action<bool> OnRevealStateChanged;
    public System.Action<RevealRenderingMode> OnRenderingModeChanged;

    // Properties
    public bool IsRevealed => isRevealed;
    public float RevealAmount => currentRevealAmount;
    public int RequiredFilters => (int)requiredFilters;
    public bool RequireAllFilters => requireAllFilters;
    public RevealRenderingMode RenderingMode => renderingMode;

    void Awake()
    {
        // Cache components
        CacheComponents();

        // Store original values
        StoreOriginalValues();

        // Setup mask renderer if needed
        SetupMaskRenderer();
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

        // Initialize rendering mode
        ApplyRenderingMode();
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

    void SetupMaskRenderer()
    {
        if (renderingMode == RevealRenderingMode.PreciseMask && autoSetupMaskRenderer)
        {
            // Add FilterMaskRenderer component if not present
            maskRenderer = GetComponent<FilterMaskRenderer>();
            if (maskRenderer == null)
            {
                maskRenderer = gameObject.AddComponent<FilterMaskRenderer>();
            }

            // Configure mask renderer settings
            if (maskRenderer != null)
            {
                maskRenderer.SetMaskResolution(maskResolution);
                maskRenderer.SetPreciseMaskingEnabled(true);
            }
        }
    }

    void ApplyRenderingMode()
    {
        switch (renderingMode)
        {
            case RevealRenderingMode.Transparency:
                // Disable mask renderer if present
                if (maskRenderer != null)
                {
                    maskRenderer.SetPreciseMaskingEnabled(false);
                }
                break;

            case RevealRenderingMode.PreciseMask:
                // Enable mask renderer
                if (maskRenderer != null)
                {
                    maskRenderer.SetPreciseMaskingEnabled(true);
                }
                else if (autoSetupMaskRenderer)
                {
                    SetupMaskRenderer();
                }
                break;
        }

        OnRenderingModeChanged?.Invoke(renderingMode);
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

            // Handle different rendering modes
            if (renderingMode == RevealRenderingMode.PreciseMask && maskRenderer != null)
            {
                // For precise mask mode, force update the mask
                maskRenderer.ForceUpdateMask();
            }

            // Invoke event
            OnRevealStateChanged?.Invoke(revealed);

            Debug.Log($"{name} reveal state changed to: {revealed} (Mode: {renderingMode})");
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

    /// <summary>
    /// 切换渲染模式
    /// </summary>
    public void SetRenderingMode(RevealRenderingMode mode)
    {
        if (renderingMode != mode)
        {
            renderingMode = mode;
            ApplyRenderingMode();
        }
    }

    /// <summary>
    /// 设置遮罩分辨率
    /// </summary>
    public void SetMaskResolution(int resolution)
    {
        maskResolution = Mathf.Clamp(resolution, 32, 1024);

        if (maskRenderer != null)
        {
            maskRenderer.SetMaskResolution(maskResolution);
        }
    }

    /// <summary>
    /// 启用/禁用遮罩LOD
    /// </summary>
    public void SetMaskLODEnabled(bool enabled)
    {
        enableMaskLOD = enabled;
        // LOD设置会在FilterMaskRenderer中自动处理
    }

    /// <summary>
    /// 设置遮罩更新间隔
    /// </summary>
    public void SetMaskUpdateInterval(float interval)
    {
        maskUpdateInterval = Mathf.Max(0.05f, interval);
        // 更新间隔会在FilterMaskRenderer中使用
    }

    /// <summary>
    /// 强制更新遮罩（仅在精确遮罩模式下有效）
    /// </summary>
    public void ForceUpdateMask()
    {
        if (renderingMode == RevealRenderingMode.PreciseMask && maskRenderer != null)
        {
            maskRenderer.ForceUpdateMask();
        }
    }

    /// <summary>
    /// 获取物体边界（供外部系统使用）
    /// </summary>
    public Bounds GetBounds()
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

    /// <summary>
    /// 获取遮罩渲染器组件
    /// </summary>
    public FilterMaskRenderer GetMaskRenderer()
    {
        return maskRenderer;
    }

    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    public string GetPerformanceInfo()
    {
        string info = $"Rendering Mode: {renderingMode}\n";
        info += $"Revealed: {isRevealed} ({currentRevealAmount:F2})\n";
        info += $"Required Filters: {requiredFilters} ({(requireAllFilters ? "AND" : "OR")})\n";

        if (maskRenderer != null)
        {
            info += $"Mask Renderer: {maskRenderer.GetPerformanceStats()}";
        }

        return info;
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

        // Draw rendering mode indicator
        if (renderingMode == RevealRenderingMode.PreciseMask)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size * 1.1f);
        }
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
        string filterInfo = $"Required: {requiredFilters}\nLogic: {(requireAllFilters ? "AND" : "OR")}\nRevealed: {isRevealed}\nAmount: {currentRevealAmount:F2}\nMode: {renderingMode}";

        if (renderingMode == RevealRenderingMode.PreciseMask)
        {
            filterInfo += $"\nMask Resolution: {maskResolution}";
            filterInfo += $"\nMask LOD: {enableMaskLOD}";

            if (maskRenderer != null)
            {
                filterInfo += $"\nMask Active: {maskRenderer.IsMaskingEnabled}";
            }
        }

        UnityEditor.Handles.Label(labelPos, filterInfo);
        #endif
    }

    #endregion
}