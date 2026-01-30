using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackgroundLayer
{
    [Header("Layer Settings")]
    public string layerName = "Background Layer";
    public GameObject layerObject;
    public SpriteRenderer spriteRenderer;

    [Header("Parallax Settings")]
    public float parallaxSpeed = 0.5f;
    public bool enableParallax = true;

    [Header("Animation Settings")]
    public bool enableAnimation = false;
    public float animationSpeed = 1f;
    public Vector2 animationDirection = Vector2.right;

    [Header("Filter Effects")]
    public bool affectedByFilters = false;
    public FilterType requiredFilters = FilterType.Red;
    public bool requireAllFilters = true;

    // Runtime state
    [System.NonSerialized]
    public Vector3 startPosition;
    [System.NonSerialized]
    public bool isVisible = true;
    [System.NonSerialized]
    public float currentAlpha = 1f;
}

public class BackgroundManager : MonoBehaviour
{
    [Header("Background Layers")]
    public List<BackgroundLayer> backgroundLayers = new List<BackgroundLayer>();

    [Header("Parallax Settings")]
    public Transform parallaxTarget; // Usually the main camera
    public bool enableGlobalParallax = true;

    [Header("Transition Settings")]
    public float layerTransitionSpeed = 2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Singleton
    public static BackgroundManager Instance { get; private set; }

    // Runtime variables
    private Vector3 lastTargetPosition;
    private Camera mainCamera;

    // Events
    public System.Action<int, bool> OnLayerVisibilityChanged;
    public System.Action<string> OnLayerOrderChanged;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeBackgroundSystem();
    }

    void Update()
    {
        if (enableGlobalParallax && parallaxTarget != null)
        {
            UpdateParallax();
        }

        UpdateAnimations();
        UpdateFilterEffects();
    }

    void InitializeBackgroundSystem()
    {
        // Get main camera if not assigned
        if (parallaxTarget == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                parallaxTarget = mainCamera.transform;
            }
        }

        // Initialize layers
        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            InitializeLayer(i);
        }

        // Store initial target position
        if (parallaxTarget != null)
        {
            lastTargetPosition = parallaxTarget.position;
        }

        Debug.Log($"Background system initialized with {backgroundLayers.Count} layers");
    }

    void InitializeLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= backgroundLayers.Count) return;

        BackgroundLayer layer = backgroundLayers[layerIndex];

        // Store start position
        if (layer.layerObject != null)
        {
            layer.startPosition = layer.layerObject.transform.position;
        }

        // Get sprite renderer if not assigned
        if (layer.spriteRenderer == null && layer.layerObject != null)
        {
            layer.spriteRenderer = layer.layerObject.GetComponent<SpriteRenderer>();
        }

        // Set initial visibility
        layer.isVisible = true;
        layer.currentAlpha = 1f;

        // Set sorting order based on layer index (higher index = front)
        if (layer.spriteRenderer != null)
        {
            layer.spriteRenderer.sortingOrder = -backgroundLayers.Count + layerIndex;
        }
    }

    #region Parallax System

    void UpdateParallax()
    {
        if (parallaxTarget == null) return;

        Vector3 deltaMovement = parallaxTarget.position - lastTargetPosition;

        foreach (BackgroundLayer layer in backgroundLayers)
        {
            if (layer.enableParallax && layer.layerObject != null)
            {
                // Apply parallax movement
                Vector3 parallaxMovement = deltaMovement * layer.parallaxSpeed;
                layer.layerObject.transform.position += parallaxMovement;
            }
        }

        lastTargetPosition = parallaxTarget.position;
    }

    #endregion

    #region Animation System

    void UpdateAnimations()
    {
        foreach (BackgroundLayer layer in backgroundLayers)
        {
            if (layer.enableAnimation && layer.layerObject != null)
            {
                // Apply continuous animation movement
                Vector3 animationMovement = (Vector3)layer.animationDirection * layer.animationSpeed * Time.deltaTime;
                layer.layerObject.transform.position += animationMovement;
            }
        }
    }

    #endregion

    #region Filter Effects

    void UpdateFilterEffects()
    {
        if (FilterSystemManager.Instance == null) return;

        foreach (BackgroundLayer layer in backgroundLayers)
        {
            if (layer.affectedByFilters && layer.layerObject != null)
            {
                UpdateLayerFilterEffect(layer);
            }
        }
    }

    void UpdateLayerFilterEffect(BackgroundLayer layer)
    {
        // Get filter effect at layer position
        Vector2 layerPos = layer.layerObject.transform.position;
        int currentEffect = FilterSystemManager.Instance.GetFilterEffectAtPosition(layerPos);

        // Check if layer should be visible
        bool shouldBeVisible = ShouldLayerBeVisible(layer, currentEffect);

        // Update visibility with smooth transition
        if (shouldBeVisible != layer.isVisible)
        {
            layer.isVisible = shouldBeVisible;
            OnLayerVisibilityChanged?.Invoke(backgroundLayers.IndexOf(layer), shouldBeVisible);
        }

        // Update alpha transition
        float targetAlpha = shouldBeVisible ? 1f : 0f;
        layer.currentAlpha = Mathf.MoveTowards(layer.currentAlpha, targetAlpha, layerTransitionSpeed * Time.deltaTime);

        // Apply alpha to sprite renderer
        if (layer.spriteRenderer != null)
        {
            Color color = layer.spriteRenderer.color;
            color.a = transitionCurve.Evaluate(layer.currentAlpha);
            layer.spriteRenderer.color = color;
        }
    }

    bool ShouldLayerBeVisible(BackgroundLayer layer, int currentFilterEffect)
    {
        int requiredValue = (int)layer.requiredFilters;

        if (layer.requireAllFilters)
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

    #endregion

    #region Layer Management

    public void SetLayerVisibility(int layerIndex, bool visible)
    {
        if (layerIndex < 0 || layerIndex >= backgroundLayers.Count) return;

        BackgroundLayer layer = backgroundLayers[layerIndex];
        layer.isVisible = visible;

        if (layer.layerObject != null)
        {
            layer.layerObject.SetActive(visible);
        }

        OnLayerVisibilityChanged?.Invoke(layerIndex, visible);
    }

    public void SetLayerVisibility(string layerName, bool visible)
    {
        int index = backgroundLayers.FindIndex(layer => layer.layerName == layerName);
        if (index >= 0)
        {
            SetLayerVisibility(index, visible);
        }
    }

    public void SetLayerAlpha(int layerIndex, float alpha)
    {
        if (layerIndex < 0 || layerIndex >= backgroundLayers.Count) return;

        BackgroundLayer layer = backgroundLayers[layerIndex];
        layer.currentAlpha = Mathf.Clamp01(alpha);

        if (layer.spriteRenderer != null)
        {
            Color color = layer.spriteRenderer.color;
            color.a = layer.currentAlpha;
            layer.spriteRenderer.color = color;
        }
    }

    public void ReorderLayer(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= backgroundLayers.Count ||
            toIndex < 0 || toIndex >= backgroundLayers.Count ||
            fromIndex == toIndex) return;

        // Move layer in list
        BackgroundLayer layer = backgroundLayers[fromIndex];
        backgroundLayers.RemoveAt(fromIndex);
        backgroundLayers.Insert(toIndex, layer);

        // Update sorting orders
        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            if (backgroundLayers[i].spriteRenderer != null)
            {
                backgroundLayers[i].spriteRenderer.sortingOrder = -backgroundLayers.Count + i;
            }
        }

        OnLayerOrderChanged?.Invoke(layer.layerName);
    }

    public void ResetLayerPosition(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= backgroundLayers.Count) return;

        BackgroundLayer layer = backgroundLayers[layerIndex];
        if (layer.layerObject != null)
        {
            layer.layerObject.transform.position = layer.startPosition;
        }
    }

    public void ResetAllLayerPositions()
    {
        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            ResetLayerPosition(i);
        }
    }

    #endregion

    #region Public Getters

    public BackgroundLayer GetLayer(int index)
    {
        if (index >= 0 && index < backgroundLayers.Count)
        {
            return backgroundLayers[index];
        }
        return null;
    }

    public BackgroundLayer GetLayer(string layerName)
    {
        return backgroundLayers.Find(layer => layer.layerName == layerName);
    }

    public int GetLayerCount()
    {
        return backgroundLayers.Count;
    }

    public bool IsLayerVisible(int layerIndex)
    {
        BackgroundLayer layer = GetLayer(layerIndex);
        return layer != null && layer.isVisible;
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        if (backgroundLayers == null) return;

        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            BackgroundLayer layer = backgroundLayers[i];
            if (layer.layerObject != null)
            {
                // Draw layer bounds
                Gizmos.color = layer.isVisible ? Color.green : Color.red;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

                Bounds bounds = GetLayerBounds(layer);
                Gizmos.DrawCube(bounds.center, bounds.size);

                // Draw layer index
                Gizmos.color = Color.white;
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(bounds.center, $"Layer {i}: {layer.layerName}");
                #endif
            }
        }
    }

    Bounds GetLayerBounds(BackgroundLayer layer)
    {
        if (layer.spriteRenderer != null)
        {
            return layer.spriteRenderer.bounds;
        }
        else if (layer.layerObject != null)
        {
            return new Bounds(layer.layerObject.transform.position, Vector3.one);
        }
        return new Bounds();
    }

    #endregion
}