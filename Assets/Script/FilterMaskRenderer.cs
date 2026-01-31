using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 滤镜遮罩渲染器 - 管理RevealableObject的遮罩渲染过程
/// </summary>
[RequireComponent(typeof(RevealableObject))]
public class FilterMaskRenderer : MonoBehaviour
{
    [Header("Mask Settings")]
    [SerializeField] private int maskResolution = 256;
    [SerializeField] private bool enablePreciseMasking = true;
    [SerializeField] private bool autoUpdateMask = true;
    [SerializeField] private float updateInterval = 0.2f;

    [Header("Performance Settings")]
    [SerializeField] private bool useDistanceCulling = true;
    [SerializeField] private float maxRenderDistance = 20f;
    [SerializeField] private bool useLOD = true;
    [SerializeField] private float lodDistance1 = 10f; // 高质量距离
    [SerializeField] private float lodDistance2 = 15f; // 中等质量距离

    [Header("Visual Settings")]
    [SerializeField] private Color revealColor = Color.white;
    [SerializeField] private Color hiddenColor = new Color(0, 0, 0, 0);
    [SerializeField] private float maskThreshold = 0.1f;
    [SerializeField] private float smoothEdge = 0.02f;
    [SerializeField] private float revealIntensity = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool logMaskUpdates = false;

    // Components
    private RevealableObject revealableObject;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material maskedMaterial;

    // Mask state
    private Texture2D currentMaskTexture;
    private bool isMaskingEnabled = false;
    private float lastUpdateTime = 0f;
    private Vector3 lastCameraPosition;
    private int lastFilterCount = 0;

    // Performance tracking
    private int maskUpdateCount = 0;
    private float totalUpdateTime = 0f;

    // Properties
    public bool IsMaskingEnabled => isMaskingEnabled && enablePreciseMasking;
    public int MaskResolution => GetCurrentMaskResolution();
    public Material MaskedMaterial => maskedMaterial;

    void Awake()
    {
        // 获取组件引用
        revealableObject = GetComponent<RevealableObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"[FilterMaskRenderer] SpriteRenderer not found on {gameObject.name}");
            enabled = false;
            return;
        }

        // 存储原始材质
        originalMaterial = spriteRenderer.material;
    }

    void Start()
    {
        // 初始化遮罩系统
        InitializeMaskingSystem();

        // 注册到滤镜系统
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.OnFilterEffectsUpdated += OnFilterEffectsUpdated;
        }
    }

    void Update()
    {
        if (!IsMaskingEnabled || !autoUpdateMask)
            return;

        // 检查是否需要更新遮罩
        if (ShouldUpdateMask())
        {
            UpdateMask();
        }
    }

    void OnDestroy()
    {
        // 清理资源
        CleanupMaskingSystem();

        // 取消注册
        if (FilterSystemManager.Instance != null)
        {
            FilterSystemManager.Instance.OnFilterEffectsUpdated -= OnFilterEffectsUpdated;
        }
    }

    #region Initialization

    /// <summary>
    /// 初始化遮罩系统
    /// </summary>
    private void InitializeMaskingSystem()
    {
        if (!enablePreciseMasking)
        {
            isMaskingEnabled = false;
            return;
        }

        // 创建遮罩材质
        CreateMaskedMaterial();

        // 初始化遮罩纹理
        UpdateMask();

        isMaskingEnabled = true;

        if (logMaskUpdates)
        {
            Debug.Log($"[FilterMaskRenderer] Masking system initialized for {gameObject.name}");
        }
    }

    /// <summary>
    /// 创建遮罩材质
    /// </summary>
    private void CreateMaskedMaterial()
    {
        Shader maskShader = Shader.Find("Custom/RevealMaskShader");
        if (maskShader == null)
        {
            Debug.LogError("[FilterMaskRenderer] RevealMaskShader not found! Make sure the shader is in the project.");
            enablePreciseMasking = false;
            return;
        }

        // 创建新的材质实例
        maskedMaterial = new Material(maskShader);

        // 设置材质属性
        maskedMaterial.SetTexture("_MainTex", originalMaterial.mainTexture);
        maskedMaterial.SetColor("_RevealColor", revealColor);
        maskedMaterial.SetColor("_HiddenColor", hiddenColor);
        maskedMaterial.SetFloat("_MaskThreshold", maskThreshold);
        maskedMaterial.SetFloat("_SmoothEdge", smoothEdge);
        maskedMaterial.SetFloat("_RevealIntensity", revealIntensity);

        // 应用遮罩材质
        spriteRenderer.material = maskedMaterial;
    }

    #endregion

    #region Mask Update Logic

    /// <summary>
    /// 检查是否应该更新遮罩
    /// </summary>
    private bool ShouldUpdateMask()
    {
        // 时间间隔检查
        if (Time.time - lastUpdateTime < updateInterval)
            return false;

        // 距离剔除检查
        if (useDistanceCulling && IsOutOfRenderDistance())
            return false;

        // 滤镜数量变化检查
        if (FilterSystemManager.Instance != null)
        {
            int currentFilterCount = FilterSystemManager.Instance.GetActiveFilters().Count;
            if (currentFilterCount != lastFilterCount)
            {
                lastFilterCount = currentFilterCount;
                return true;
            }
        }

        // 摄像机位置变化检查（用于LOD）
        if (Camera.main != null)
        {
            Vector3 currentCameraPos = Camera.main.transform.position;
            if (Vector3.Distance(currentCameraPos, lastCameraPosition) > 1f)
            {
                lastCameraPosition = currentCameraPos;
                return true;
            }
        }

        return true; // 默认更新
    }

    /// <summary>
    /// 更新遮罩纹理
    /// </summary>
    public void UpdateMask()
    {
        if (!IsMaskingEnabled || FilterSystemManager.Instance == null)
            return;

        float startTime = Time.realtimeSinceStartup;

        // 获取当前激活的滤镜
        List<FilterController> activeFilters = FilterSystemManager.Instance.GetActiveFilters();

        // 生成遮罩纹理
        int resolution = GetCurrentMaskResolution();
        Texture2D newMaskTexture = MaskTextureGenerator.GenerateMaskTexture(
            revealableObject, activeFilters, resolution);

        // 更新材质
        if (newMaskTexture != null && maskedMaterial != null)
        {
            maskedMaterial.SetTexture("_MaskTex", newMaskTexture);
            currentMaskTexture = newMaskTexture;
        }

        // 更新统计信息
        lastUpdateTime = Time.time;
        maskUpdateCount++;
        totalUpdateTime += Time.realtimeSinceStartup - startTime;

        if (logMaskUpdates)
        {
            float updateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log($"[FilterMaskRenderer] Mask updated for {gameObject.name} in {updateTime:F2}ms (Resolution: {resolution})");
        }
    }

    /// <summary>
    /// 强制更新遮罩（忽略时间间隔）
    /// </summary>
    public void ForceUpdateMask()
    {
        lastUpdateTime = 0f;
        UpdateMask();
    }

    #endregion

    #region LOD and Performance

    /// <summary>
    /// 获取当前应使用的遮罩分辨率（基于LOD）
    /// </summary>
    private int GetCurrentMaskResolution()
    {
        if (!useLOD || Camera.main == null)
            return maskResolution;

        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        if (distance <= lodDistance1)
        {
            return maskResolution; // 高质量
        }
        else if (distance <= lodDistance2)
        {
            return Mathf.Max(64, maskResolution / 2); // 中等质量
        }
        else
        {
            return Mathf.Max(32, maskResolution / 4); // 低质量
        }
    }

    /// <summary>
    /// 检查是否超出渲染距离
    /// </summary>
    private bool IsOutOfRenderDistance()
    {
        if (Camera.main == null)
            return false;

        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        return distance > maxRenderDistance;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 滤镜效果更新事件处理
    /// </summary>
    private void OnFilterEffectsUpdated()
    {
        if (autoUpdateMask)
        {
            // 延迟一帧更新，避免同一帧多次更新
            if (Time.time > lastUpdateTime + 0.01f)
            {
                UpdateMask();
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 启用/禁用精确遮罩
    /// </summary>
    public void SetPreciseMaskingEnabled(bool enabled)
    {
        if (enablePreciseMasking == enabled)
            return;

        enablePreciseMasking = enabled;

        if (enabled)
        {
            InitializeMaskingSystem();
        }
        else
        {
            DisableMasking();
        }
    }

    /// <summary>
    /// 设置遮罩分辨率
    /// </summary>
    public void SetMaskResolution(int resolution)
    {
        maskResolution = Mathf.Clamp(resolution, 32, 1024);
        if (IsMaskingEnabled)
        {
            ForceUpdateMask();
        }
    }

    /// <summary>
    /// 设置视觉属性
    /// </summary>
    public void SetVisualProperties(Color reveal, Color hidden, float threshold, float edge, float intensity)
    {
        revealColor = reveal;
        hiddenColor = hidden;
        maskThreshold = threshold;
        smoothEdge = edge;
        revealIntensity = intensity;

        if (maskedMaterial != null)
        {
            maskedMaterial.SetColor("_RevealColor", revealColor);
            maskedMaterial.SetColor("_HiddenColor", hiddenColor);
            maskedMaterial.SetFloat("_MaskThreshold", maskThreshold);
            maskedMaterial.SetFloat("_SmoothEdge", smoothEdge);
            maskedMaterial.SetFloat("_RevealIntensity", revealIntensity);
        }
    }

    /// <summary>
    /// 禁用遮罩渲染
    /// </summary>
    public void DisableMasking()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        isMaskingEnabled = false;
    }

    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    public string GetPerformanceStats()
    {
        if (maskUpdateCount == 0)
            return "No mask updates yet";

        float avgUpdateTime = (totalUpdateTime / maskUpdateCount) * 1000f;
        return $"Mask Updates: {maskUpdateCount}, Avg Time: {avgUpdateTime:F2}ms, Current Resolution: {GetCurrentMaskResolution()}";
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// 清理遮罩系统资源
    /// </summary>
    private void CleanupMaskingSystem()
    {
        // 恢复原始材质
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }

        // 销毁遮罩材质
        if (maskedMaterial != null)
        {
            DestroyImmediate(maskedMaterial);
            maskedMaterial = null;
        }

        // 清理当前遮罩纹理（注意：不要销毁，因为它可能被纹理池管理）
        currentMaskTexture = null;
    }

    #endregion

    #region Debug and Gizmos

    void OnDrawGizmos()
    {
        if (!showDebugInfo)
            return;

        // 绘制渲染距离
        if (useDistanceCulling)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxRenderDistance);
        }

        // 绘制LOD距离
        if (useLOD)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, lodDistance1);
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, lodDistance2);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo)
            return;

        // 显示调试信息
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width &&
            screenPos.y >= 0 && screenPos.y <= Screen.height)
        {
            Vector2 guiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

            GUI.color = Color.white;
            GUI.Label(new Rect(guiPos.x, guiPos.y, 200, 60),
                $"Masking: {IsMaskingEnabled}\n" +
                $"Resolution: {GetCurrentMaskResolution()}\n" +
                $"Updates: {maskUpdateCount}");
        }
    }

    #endregion
}