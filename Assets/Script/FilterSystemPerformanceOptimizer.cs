using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 滤镜系统性能优化工具
/// 提供性能监控、优化建议和自动优化功能
/// </summary>
public class FilterSystemPerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Monitoring")]
    public bool enableContinuousMonitoring = false;
    public float monitoringInterval = 1.0f;
    public int performanceHistorySize = 60;

    [Header("Optimization Settings")]
    public bool enableAutoOptimization = true;
    public float targetFPS = 60.0f;
    public float lowPerformanceThreshold = 30.0f;

    [Header("Debug Display")]
    public bool showPerformanceOverlay = false;
    public KeyCode toggleOverlayKey = KeyCode.F1;

    // 性能数据
    private Queue<float> fpsHistory = new Queue<float>();
    private Queue<float> memoryHistory = new Queue<float>();
    private Queue<int> filterCountHistory = new Queue<int>();

    // 优化状态
    private bool isOptimizationActive = false;
    private float lastOptimizationTime = 0f;
    private const float OPTIMIZATION_COOLDOWN = 5.0f;

    // 性能统计
    private float averageFPS = 0f;
    private float averageMemoryUsage = 0f;
    private int peakFilterCount = 0;

    // GUI样式
    private GUIStyle overlayStyle;
    private bool guiStyleInitialized = false;

    void Start()
    {
        if (enableContinuousMonitoring)
        {
            StartCoroutine(ContinuousPerformanceMonitoring());
        }
    }

    void Update()
    {
        // 切换性能覆盖显示
        if (Input.GetKeyDown(toggleOverlayKey))
        {
            showPerformanceOverlay = !showPerformanceOverlay;
        }

        // 自动优化检查
        if (enableAutoOptimization && Time.time - lastOptimizationTime > OPTIMIZATION_COOLDOWN)
        {
            CheckAndOptimizePerformance();
        }
    }

    /// <summary>
    /// 持续性能监控协程
    /// </summary>
    private IEnumerator ContinuousPerformanceMonitoring()
    {
        while (enableContinuousMonitoring)
        {
            CollectPerformanceData();
            yield return new WaitForSeconds(monitoringInterval);
        }
    }

    /// <summary>
    /// 收集性能数据
    /// </summary>
    private void CollectPerformanceData()
    {
        // FPS数据
        float currentFPS = 1.0f / Time.deltaTime;
        fpsHistory.Enqueue(currentFPS);
        if (fpsHistory.Count > performanceHistorySize)
        {
            fpsHistory.Dequeue();
        }

        // 内存数据
        float memoryMB = Profiler.GetTotalAllocatedMemory(false) / (1024f * 1024f);
        memoryHistory.Enqueue(memoryMB);
        if (memoryHistory.Count > performanceHistorySize)
        {
            memoryHistory.Dequeue();
        }

        // 滤镜数量数据
        int filterCount = 0;
        if (FilterSystemManager.Instance != null)
        {
            filterCount = FilterSystemManager.Instance.GetActiveFilters().Count;
        }
        filterCountHistory.Enqueue(filterCount);
        if (filterCountHistory.Count > performanceHistorySize)
        {
            filterCountHistory.Dequeue();
        }

        // 更新统计数据
        UpdatePerformanceStatistics();
    }

    /// <summary>
    /// 更新性能统计数据
    /// </summary>
    private void UpdatePerformanceStatistics()
    {
        // 计算平均FPS
        if (fpsHistory.Count > 0)
        {
            float totalFPS = 0f;
            foreach (float fps in fpsHistory)
            {
                totalFPS += fps;
            }
            averageFPS = totalFPS / fpsHistory.Count;
        }

        // 计算平均内存使用
        if (memoryHistory.Count > 0)
        {
            float totalMemory = 0f;
            foreach (float memory in memoryHistory)
            {
                totalMemory += memory;
            }
            averageMemoryUsage = totalMemory / memoryHistory.Count;
        }

        // 计算峰值滤镜数量
        peakFilterCount = 0;
        foreach (int count in filterCountHistory)
        {
            if (count > peakFilterCount)
            {
                peakFilterCount = count;
            }
        }
    }

    /// <summary>
    /// 检查并优化性能
    /// </summary>
    private void CheckAndOptimizePerformance()
    {
        if (FilterSystemManager.Instance == null) return;

        float currentFPS = 1.0f / Time.deltaTime;

        // 如果性能低于阈值，启动优化
        if (currentFPS < lowPerformanceThreshold && !isOptimizationActive)
        {
            StartCoroutine(OptimizePerformance());
        }
    }

    /// <summary>
    /// 执行性能优化
    /// </summary>
    private IEnumerator OptimizePerformance()
    {
        isOptimizationActive = true;
        lastOptimizationTime = Time.time;

        Debug.Log("[PerformanceOptimizer] Starting performance optimization...");

        // 优化1: 调整实时更新频率
        if (FilterSystemManager.Instance.enableRealTimeUpdates)
        {
            float currentInterval = FilterSystemManager.Instance.updateInterval;
            float newInterval = Mathf.Min(currentInterval * 1.5f, 0.5f);
            FilterSystemManager.Instance.SetUpdateInterval(newInterval);
            Debug.Log($"[PerformanceOptimizer] Adjusted update interval from {currentInterval:F2}s to {newInterval:F2}s");
        }

        yield return new WaitForSeconds(1.0f);

        // 优化2: 强制垃圾回收
        System.GC.Collect();
        Debug.Log("[PerformanceOptimizer] Forced garbage collection");

        yield return new WaitForSeconds(1.0f);

        // 优化3: 检查是否需要禁用某些功能
        float testFPS = 1.0f / Time.deltaTime;
        if (testFPS < lowPerformanceThreshold * 0.8f)
        {
            // 极低性能情况下的激进优化
            Debug.Log("[PerformanceOptimizer] Applying aggressive optimizations...");

            // 可以在这里添加更激进的优化措施
            // 例如：减少遮罩分辨率、禁用某些视觉效果等
        }

        isOptimizationActive = false;
        Debug.Log("[PerformanceOptimizer] Performance optimization completed");
    }

    /// <summary>
    /// 获取性能报告
    /// </summary>
    public string GetPerformanceReport()
    {
        string report = "=== PERFORMANCE REPORT ===\n";
        report += $"Average FPS: {averageFPS:F1}\n";
        report += $"Average Memory: {averageMemoryUsage:F1} MB\n";
        report += $"Peak Filter Count: {peakFilterCount}\n";
        report += $"Data Points: {fpsHistory.Count}/{performanceHistorySize}\n";

        if (FilterSystemManager.Instance != null)
        {
            report += "\n" + FilterSystemManager.Instance.GetSystemPerformanceInfo();
        }

        // 性能评级
        string performanceGrade = GetPerformanceGrade();
        report += $"\nPerformance Grade: {performanceGrade}\n";

        // 优化建议
        List<string> recommendations = GetOptimizationRecommendations();
        if (recommendations.Count > 0)
        {
            report += "\nOptimization Recommendations:\n";
            foreach (string recommendation in recommendations)
            {
                report += $"• {recommendation}\n";
            }
        }

        return report;
    }

    /// <summary>
    /// 获取性能评级
    /// </summary>
    private string GetPerformanceGrade()
    {
        if (averageFPS >= targetFPS * 0.9f)
            return "A (Excellent)";
        else if (averageFPS >= targetFPS * 0.7f)
            return "B (Good)";
        else if (averageFPS >= targetFPS * 0.5f)
            return "C (Fair)";
        else if (averageFPS >= lowPerformanceThreshold)
            return "D (Poor)";
        else
            return "F (Critical)";
    }

    /// <summary>
    /// 获取优化建议
    /// </summary>
    private List<string> GetOptimizationRecommendations()
    {
        List<string> recommendations = new List<string>();

        if (averageFPS < targetFPS * 0.8f)
        {
            recommendations.Add("Consider increasing the real-time update interval");
        }

        if (averageMemoryUsage > 100f)
        {
            recommendations.Add("Memory usage is high, consider optimizing texture sizes");
        }

        if (peakFilterCount > 10)
        {
            recommendations.Add("High filter count detected, consider filter pooling");
        }

        if (FilterSystemManager.Instance != null)
        {
            var activeFilters = FilterSystemManager.Instance.GetActiveFilters();
            var revealableObjects = FilterSystemManager.Instance.GetRevealableObjects();

            if (revealableObjects.Count > 50)
            {
                recommendations.Add("Large number of revealable objects, consider object culling");
            }

            if (activeFilters.Count > 5)
            {
                recommendations.Add("Multiple active filters, consider filter merging");
            }
        }

        return recommendations;
    }

    /// <summary>
    /// 重置性能数据
    /// </summary>
    [ContextMenu("Reset Performance Data")]
    public void ResetPerformanceData()
    {
        fpsHistory.Clear();
        memoryHistory.Clear();
        filterCountHistory.Clear();
        averageFPS = 0f;
        averageMemoryUsage = 0f;
        peakFilterCount = 0;
        Debug.Log("[PerformanceOptimizer] Performance data reset");
    }

    /// <summary>
    /// 手动触发优化
    /// </summary>
    [ContextMenu("Manual Optimization")]
    public void TriggerManualOptimization()
    {
        if (!isOptimizationActive)
        {
            StartCoroutine(OptimizePerformance());
        }
    }

    /// <summary>
    /// 性能覆盖显示
    /// </summary>
    void OnGUI()
    {
        if (!showPerformanceOverlay) return;

        // 初始化GUI样式
        if (!guiStyleInitialized)
        {
            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));
            overlayStyle.normal.textColor = Color.white;
            overlayStyle.fontSize = 12;
            overlayStyle.padding = new RectOffset(10, 10, 10, 10);
            guiStyleInitialized = true;
        }

        // 显示性能信息
        float currentFPS = 1.0f / Time.deltaTime;
        float currentMemory = Profiler.GetTotalAllocatedMemory(false) / (1024f * 1024f);

        string overlayText = $"FPS: {currentFPS:F1} (Avg: {averageFPS:F1})\n";
        overlayText += $"Memory: {currentMemory:F1} MB (Avg: {averageMemoryUsage:F1})\n";

        if (FilterSystemManager.Instance != null)
        {
            overlayText += $"Active Filters: {FilterSystemManager.Instance.GetActiveFilters().Count}\n";
            overlayText += $"Revealable Objects: {FilterSystemManager.Instance.GetRevealableObjects().Count}\n";
        }

        overlayText += $"Grade: {GetPerformanceGrade()}\n";
        overlayText += $"Press {toggleOverlayKey} to toggle";

        GUI.Box(new Rect(10, 10, 300, 120), overlayText, overlayStyle);
    }

    /// <summary>
    /// 创建纯色纹理（用于GUI背景）
    /// </summary>
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    void OnDestroy()
    {
        // 清理资源
        if (overlayStyle?.normal?.background != null)
        {
            DestroyImmediate(overlayStyle.normal.background);
        }
    }
}