using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterSystemManager : MonoBehaviour
{
    [Header("System Settings")]
    public LayerMask revealableObjectLayer = 1;

    [Header("Real-time Update Settings")]
    public bool enableRealTimeUpdates = true;
    public float updateInterval = 0.15f;

    // 单例模式
    public static FilterSystemManager Instance { get; private set; }

    // 当前场景中的所有滤镜和可显示物体
    private List<FilterController> activeFilters = new List<FilterController>();
    private List<RevealableObject> revealableObjects = new List<RevealableObject>();

    // 滤镜效果缓存
    private Dictionary<Vector2Int, int> filterEffectMap = new Dictionary<Vector2Int, int>();

    // 实时更新系统
    private Coroutine updateCoroutine;
    private bool isRealTimeUpdateEnabled = true;

    // 事件系统
    public System.Action OnFilterEffectsUpdated;
    public System.Action<FilterController> OnFilterRegistered;
    public System.Action<FilterController> OnFilterUnregistered;
    public System.Action<RevealableObject> OnRevealableObjectRegistered;
    public System.Action<RevealableObject> OnRevealableObjectUnregistered;

    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 初始化时找到所有可显示物体
        RefreshRevealableObjects();

        // 启动实时更新协程
        if (enableRealTimeUpdates)
        {
            StartRealTimeUpdates();
        }
    }

    void OnDestroy()
    {
        // 停止实时更新协程
        StopRealTimeUpdates();
    }

    #region 实时更新系统

    /// <summary>
    /// 启动实时更新协程
    /// </summary>
    public void StartRealTimeUpdates()
    {
        if (updateCoroutine == null && isRealTimeUpdateEnabled && enableRealTimeUpdates)
        {
            updateCoroutine = StartCoroutine(RealTimeUpdateCoroutine());
            Debug.Log("[FilterSystemManager] Real-time updates started");
        }
    }

    /// <summary>
    /// 停止实时更新协程
    /// </summary>
    public void StopRealTimeUpdates()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
            Debug.Log("[FilterSystemManager] Real-time updates stopped");
        }
    }

    /// <summary>
    /// 实时更新协程
    /// </summary>
    private IEnumerator RealTimeUpdateCoroutine()
    {
        while (isRealTimeUpdateEnabled && enableRealTimeUpdates)
        {
            yield return new WaitForSeconds(updateInterval);
            CheckForFilterPositionChanges();
        }
    }

    /// <summary>
    /// 检查滤镜位置变化
    /// </summary>
    private void CheckForFilterPositionChanges()
    {
        bool hasAnyFilterMoved = false;

        foreach (FilterController filter in activeFilters)
        {
            if (filter != null && filter.HasPositionChanged())
            {
                hasAnyFilterMoved = true;
                filter.ResetPositionTracking();
            }
        }

        if (hasAnyFilterMoved)
        {
            UpdateFilterEffects();
        }
    }

    /// <summary>
    /// 设置实时更新启用状态
    /// </summary>
    public void SetRealTimeUpdatesEnabled(bool enabled)
    {
        if (enableRealTimeUpdates != enabled)
        {
            enableRealTimeUpdates = enabled;

            if (enabled)
            {
                StartRealTimeUpdates();
            }
            else
            {
                StopRealTimeUpdates();
            }
        }
    }

    /// <summary>
    /// 设置更新间隔
    /// </summary>
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval);

        // 重启协程以应用新的间隔
        if (updateCoroutine != null)
        {
            StopRealTimeUpdates();
            StartRealTimeUpdates();
        }
    }

    #endregion

    #region 滤镜管理

    public void RegisterFilter(FilterController filter)
    {
        if (!activeFilters.Contains(filter))
        {
            activeFilters.Add(filter);
            OnFilterRegistered?.Invoke(filter);
            UpdateFilterEffects();
            Debug.Log($"[FilterSystemManager] Filter registered: {filter.name}");
        }
    }

    public void UnregisterFilter(FilterController filter)
    {
        if (activeFilters.Remove(filter))
        {
            OnFilterUnregistered?.Invoke(filter);
            UpdateFilterEffects();
            Debug.Log($"[FilterSystemManager] Filter unregistered: {filter.name}");
        }
    }

    public void UpdateFilterEffects()
    {
        // 清空当前效果映射
        filterEffectMap.Clear();

        // 计算每个位置的滤镜效果组合
        foreach (FilterController filter in activeFilters)
        {
            if (filter.IsActive)
            {
                AddFilterEffectToMap(filter);
            }
        }

        // 更新所有可显示物体的状态
        UpdateRevealableObjects();

        // 触发事件通知遮罩系统更新
        OnFilterEffectsUpdated?.Invoke();
    }

    private void AddFilterEffectToMap(FilterController filter)
    {
        Vector2 filterPos = filter.transform.position;
        float radius = filter.FilterData.filterRadius;
        int filterValue = (int)filter.FilterData.filterType;

        // 使用网格系统优化性能
        int gridSize = 1; // 可以根据需要调整精度
        int minX = Mathf.FloorToInt((filterPos.x - radius) / gridSize);
        int maxX = Mathf.CeilToInt((filterPos.x + radius) / gridSize);
        int minY = Mathf.FloorToInt((filterPos.y - radius) / gridSize);
        int maxY = Mathf.CeilToInt((filterPos.y + radius) / gridSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 gridPos = new Vector2(x * gridSize, y * gridSize);
                float distance = Vector2.Distance(filterPos, gridPos);

                if (distance <= radius)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);

                    if (filterEffectMap.ContainsKey(gridKey))
                    {
                        // 使用位运算组合滤镜效果
                        filterEffectMap[gridKey] |= filterValue;
                    }
                    else
                    {
                        filterEffectMap[gridKey] = filterValue;
                    }
                }
            }
        }
    }

    #endregion

    #region 可显示物体管理

    public void RegisterRevealableObject(RevealableObject obj)
    {
        if (!revealableObjects.Contains(obj))
        {
            revealableObjects.Add(obj);
            OnRevealableObjectRegistered?.Invoke(obj);

            // 立即更新新注册的物体状态
            UpdateSingleRevealableObject(obj);

            Debug.Log($"[FilterSystemManager] RevealableObject registered: {obj.name}");
        }
    }

    public void UnregisterRevealableObject(RevealableObject obj)
    {
        if (revealableObjects.Remove(obj))
        {
            OnRevealableObjectUnregistered?.Invoke(obj);
            Debug.Log($"[FilterSystemManager] RevealableObject unregistered: {obj.name}");
        }
    }

    public void RefreshRevealableObjects()
    {
        revealableObjects.Clear();
        RevealableObject[] objects = FindObjectsOfType<RevealableObject>();
        revealableObjects.AddRange(objects);

        Debug.Log($"[FilterSystemManager] Refreshed {revealableObjects.Count} RevealableObjects");
    }

    private void UpdateRevealableObjects()
    {
        foreach (RevealableObject obj in revealableObjects)
        {
            if (obj != null)
            {
                UpdateSingleRevealableObject(obj);
            }
        }
    }

    private void UpdateSingleRevealableObject(RevealableObject obj)
    {
        Vector2 objPos = obj.transform.position;
        Vector2Int gridKey = new Vector2Int(
            Mathf.FloorToInt(objPos.x),
            Mathf.FloorToInt(objPos.y)
        );

        int currentEffect = 0;
        if (filterEffectMap.ContainsKey(gridKey))
        {
            currentEffect = filterEffectMap[gridKey];
        }

        // 检查物体是否应该被显示
        bool shouldReveal = obj.ShouldReveal(currentEffect);
        obj.SetRevealed(shouldReveal);

        // 如果物体使用精确遮罩模式，通知其更新遮罩
        if (obj.GetComponent<FilterMaskRenderer>() != null)
        {
            var maskRenderer = obj.GetComponent<FilterMaskRenderer>();
            if (maskRenderer.IsMaskingEnabled)
            {
                maskRenderer.RequestMaskUpdate();
            }
        }
    }

    #endregion

    #region 公共查询方法

    public int GetFilterEffectAtPosition(Vector2 position)
    {
        Vector2Int gridKey = new Vector2Int(
            Mathf.FloorToInt(position.x),
            Mathf.FloorToInt(position.y)
        );

        return filterEffectMap.ContainsKey(gridKey) ? filterEffectMap[gridKey] : 0;
    }

    public bool IsPositionRevealed(Vector2 position, int requiredFilters)
    {
        int currentEffect = GetFilterEffectAtPosition(position);
        return (currentEffect & requiredFilters) == requiredFilters;
    }

    public List<FilterController> GetActiveFilters()
    {
        return new List<FilterController>(activeFilters);
    }

    public List<RevealableObject> GetRevealableObjects()
    {
        return new List<RevealableObject>(revealableObjects);
    }

    /// <summary>
    /// 获取指定区域内的滤镜效果
    /// </summary>
    public Dictionary<Vector2Int, int> GetFilterEffectsInBounds(Bounds bounds)
    {
        Dictionary<Vector2Int, int> effectsInBounds = new Dictionary<Vector2Int, int>();

        int minX = Mathf.FloorToInt(bounds.min.x);
        int maxX = Mathf.CeilToInt(bounds.max.x);
        int minY = Mathf.FloorToInt(bounds.min.y);
        int maxY = Mathf.CeilToInt(bounds.max.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int gridKey = new Vector2Int(x, y);
                if (filterEffectMap.ContainsKey(gridKey))
                {
                    effectsInBounds[gridKey] = filterEffectMap[gridKey];
                }
            }
        }

        return effectsInBounds;
    }

    /// <summary>
    /// 强制更新所有遮罩渲染器
    /// </summary>
    public void ForceUpdateAllMasks()
    {
        foreach (RevealableObject obj in revealableObjects)
        {
            if (obj != null)
            {
                var maskRenderer = obj.GetComponent<FilterMaskRenderer>();
                if (maskRenderer != null && maskRenderer.IsMaskingEnabled)
                {
                    maskRenderer.ForceUpdateMask();
                }
            }
        }
    }

    /// <summary>
    /// 获取系统性能统计信息
    /// </summary>
    public string GetSystemPerformanceInfo()
    {
        string info = "=== Filter System Performance ===\n";
        info += $"Active Filters: {activeFilters.Count}\n";
        info += $"Revealable Objects: {revealableObjects.Count}\n";
        info += $"Filter Effect Map Size: {filterEffectMap.Count}\n";
        info += $"Real-time Updates: {(enableRealTimeUpdates ? "Enabled" : "Disabled")}\n";
        info += $"Update Interval: {updateInterval:F2}s\n";

        // 统计遮罩渲染器
        int maskRendererCount = 0;
        int activeMaskCount = 0;
        foreach (RevealableObject obj in revealableObjects)
        {
            if (obj != null)
            {
                var maskRenderer = obj.GetComponent<FilterMaskRenderer>();
                if (maskRenderer != null)
                {
                    maskRendererCount++;
                    if (maskRenderer.IsMaskingEnabled)
                    {
                        activeMaskCount++;
                    }
                }
            }
        }

        info += $"Mask Renderers: {maskRendererCount} (Active: {activeMaskCount})\n";

        return info;
    }

    #endregion

    #region 调试功能

    void OnDrawGizmos()
    {
        if (Application.isPlaying && filterEffectMap != null)
        {
            // 绘制滤镜效果网格
            Gizmos.color = Color.yellow;
            foreach (var kvp in filterEffectMap)
            {
                Vector2 worldPos = new Vector2(kvp.Key.x, kvp.Key.y);
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.8f);
            }

            // 绘制实时更新状态指示器
            if (enableRealTimeUpdates && updateCoroutine != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 1.0f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // 显示系统统计信息
            #if UNITY_EDITOR
            Vector3 labelPos = transform.position + Vector3.up * 2.0f;
            string systemInfo = GetSystemPerformanceInfo();
            UnityEditor.Handles.Label(labelPos, systemInfo);
            #endif
        }
    }

    /// <summary>
    /// 在控制台输出系统状态（用于调试）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogSystemStatus()
    {
        Debug.Log(GetSystemPerformanceInfo());
    }

    #endregion
}