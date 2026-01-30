using System.Collections.Generic;
using UnityEngine;

public class FilterSystemManager : MonoBehaviour
{
    [Header("System Settings")]
    public LayerMask revealableObjectLayer = 1;

    // 单例模式
    public static FilterSystemManager Instance { get; private set; }

    // 当前场景中的所有滤镜和可显示物体
    private List<FilterController> activeFilters = new List<FilterController>();
    private List<RevealableObject> revealableObjects = new List<RevealableObject>();

    // 滤镜效果缓存
    private Dictionary<Vector2Int, int> filterEffectMap = new Dictionary<Vector2Int, int>();

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
    }

    #region 滤镜管理

    public void RegisterFilter(FilterController filter)
    {
        if (!activeFilters.Contains(filter))
        {
            activeFilters.Add(filter);
            UpdateFilterEffects();
        }
    }

    public void UnregisterFilter(FilterController filter)
    {
        if (activeFilters.Remove(filter))
        {
            UpdateFilterEffects();
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
        }
    }

    public void UnregisterRevealableObject(RevealableObject obj)
    {
        revealableObjects.Remove(obj);
    }

    public void RefreshRevealableObjects()
    {
        revealableObjects.Clear();
        RevealableObject[] objects = FindObjectsOfType<RevealableObject>();
        revealableObjects.AddRange(objects);
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
        }
    }

    #endregion
}