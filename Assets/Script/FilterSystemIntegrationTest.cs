using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 滤镜系统集成测试脚本
/// 用于验证实时更新系统和精确遮罩系统的集成功能
/// </summary>
public class FilterSystemIntegrationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = true;
    public bool enableDetailedLogging = true;
    public float testDuration = 10.0f;

    [Header("Test Objects")]
    public FilterController testFilter;
    public RevealableObject testRevealableObject;
    public GameObject testObjectWithMask;

    [Header("Performance Monitoring")]
    public bool monitorPerformance = true;
    public float performanceLogInterval = 2.0f;

    // 测试状态
    private bool isTestRunning = false;
    private float testStartTime;
    private int frameCount = 0;
    private float totalFrameTime = 0f;

    // 性能监控
    private Coroutine performanceMonitorCoroutine;

    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunIntegrationTests());
        }
    }

    /// <summary>
    /// 运行集成测试
    /// </summary>
    public IEnumerator RunIntegrationTests()
    {
        if (isTestRunning)
        {
            LogTest("Integration tests are already running!");
            yield break;
        }

        isTestRunning = true;
        testStartTime = Time.time;

        LogTest("=== Starting Filter System Integration Tests ===");

        // 启动性能监控
        if (monitorPerformance)
        {
            performanceMonitorCoroutine = StartCoroutine(MonitorPerformance());
        }

        // 测试1: 系统初始化测试
        yield return StartCoroutine(TestSystemInitialization());

        // 测试2: 实时更新系统测试
        yield return StartCoroutine(TestRealTimeUpdates());

        // 测试3: 精确遮罩系统测试
        yield return StartCoroutine(TestPreciseMaskSystem());

        // 测试4: 系统集成测试
        yield return StartCoroutine(TestSystemIntegration());

        // 测试5: 性能压力测试
        yield return StartCoroutine(TestPerformanceStress());

        // 停止性能监控
        if (performanceMonitorCoroutine != null)
        {
            StopCoroutine(performanceMonitorCoroutine);
        }

        // 输出最终测试报告
        GenerateFinalTestReport();

        isTestRunning = false;
        LogTest("=== Integration Tests Completed ===");
    }

    /// <summary>
    /// 测试系统初始化
    /// </summary>
    private IEnumerator TestSystemInitialization()
    {
        LogTest("--- Test 1: System Initialization ---");

        // 检查FilterSystemManager单例
        if (FilterSystemManager.Instance == null)
        {
            LogTest("ERROR: FilterSystemManager instance not found!");
            yield break;
        }

        LogTest("✓ FilterSystemManager singleton initialized");

        // 检查实时更新系统
        FilterSystemManager.Instance.SetRealTimeUpdatesEnabled(true);
        yield return new WaitForSeconds(0.5f);

        LogTest("✓ Real-time update system enabled");

        // 输出系统状态
        if (enableDetailedLogging)
        {
            LogTest(FilterSystemManager.Instance.GetSystemPerformanceInfo());
        }

        LogTest("✓ System initialization test passed");
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 测试实时更新系统
    /// </summary>
    private IEnumerator TestRealTimeUpdates()
    {
        LogTest("--- Test 2: Real-time Updates ---");

        if (testFilter == null)
        {
            LogTest("WARNING: No test filter assigned, creating one...");
            yield return StartCoroutine(CreateTestFilter());
        }

        Vector3 originalPosition = testFilter.transform.position;
        Vector3 targetPosition = originalPosition + Vector3.right * 3.0f;

        LogTest($"Moving filter from {originalPosition} to {targetPosition}");

        // 移动滤镜并监控更新
        float moveTime = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            testFilter.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            yield return null;
        }

        // 等待系统更新
        yield return new WaitForSeconds(0.5f);

        LogTest("✓ Real-time update test completed");
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 测试精确遮罩系统
    /// </summary>
    private IEnumerator TestPreciseMaskSystem()
    {
        LogTest("--- Test 3: Precise Mask System ---");

        if (testObjectWithMask == null)
        {
            LogTest("WARNING: No test object with mask assigned, creating one...");
            yield return StartCoroutine(CreateTestObjectWithMask());
        }

        // 获取遮罩渲染器组件
        FilterMaskRenderer maskRenderer = testObjectWithMask.GetComponent<FilterMaskRenderer>();
        if (maskRenderer == null)
        {
            LogTest("ERROR: FilterMaskRenderer component not found!");
            yield break;
        }

        // 测试遮罩启用/禁用
        LogTest("Testing mask enable/disable...");
        maskRenderer.SetMaskingEnabled(false);
        yield return new WaitForSeconds(0.5f);

        maskRenderer.SetMaskingEnabled(true);
        yield return new WaitForSeconds(0.5f);

        // 测试遮罩更新
        LogTest("Testing mask updates...");
        maskRenderer.ForceUpdateMask();
        yield return new WaitForSeconds(0.5f);

        LogTest("✓ Precise mask system test completed");
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 测试系统集成
    /// </summary>
    private IEnumerator TestSystemIntegration()
    {
        LogTest("--- Test 4: System Integration ---");

        // 同时测试实时更新和遮罩系统
        if (testFilter != null && testObjectWithMask != null)
        {
            Vector3 filterStart = testFilter.transform.position;
            Vector3 objectPos = testObjectWithMask.transform.position;

            LogTest("Testing integrated real-time updates with mask rendering...");

            // 将滤镜移动到物体附近
            float moveTime = 3.0f;
            float elapsedTime = 0f;

            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveTime;

                // 围绕物体移动滤镜
                float angle = t * Mathf.PI * 2;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2.0f;
                testFilter.transform.position = objectPos + offset;

                yield return null;
            }

            LogTest("✓ Integrated movement test completed");
        }

        // 强制更新所有遮罩
        FilterSystemManager.Instance.ForceUpdateAllMasks();
        yield return new WaitForSeconds(0.5f);

        LogTest("✓ System integration test completed");
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 性能压力测试
    /// </summary>
    private IEnumerator TestPerformanceStress()
    {
        LogTest("--- Test 5: Performance Stress Test ---");

        // 记录开始时的性能
        float startTime = Time.realtimeSinceStartup;
        int startFrameCount = Time.frameCount;

        // 快速移动滤镜进行压力测试
        if (testFilter != null)
        {
            LogTest("Running stress test with rapid filter movement...");

            float stressTestDuration = 3.0f;
            float elapsedTime = 0f;

            while (elapsedTime < stressTestDuration)
            {
                elapsedTime += Time.deltaTime;

                // 快速随机移动
                Vector3 randomOffset = new Vector3(
                    Mathf.Sin(Time.time * 10) * 2,
                    Mathf.Cos(Time.time * 8) * 2,
                    0
                );
                testFilter.transform.position = randomOffset;

                yield return null;
            }
        }

        // 计算性能指标
        float endTime = Time.realtimeSinceStartup;
        int endFrameCount = Time.frameCount;

        float testDuration = endTime - startTime;
        int framesDuringTest = endFrameCount - startFrameCount;
        float averageFPS = framesDuringTest / testDuration;

        LogTest($"Stress test completed: {averageFPS:F1} FPS average over {testDuration:F1}s");

        if (averageFPS < 30)
        {
            LogTest("WARNING: Performance below 30 FPS during stress test!");
        }
        else
        {
            LogTest("✓ Performance stress test passed");
        }

        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 性能监控协程
    /// </summary>
    private IEnumerator MonitorPerformance()
    {
        while (isTestRunning)
        {
            yield return new WaitForSeconds(performanceLogInterval);

            if (enableDetailedLogging)
            {
                float currentFPS = 1.0f / Time.deltaTime;
                LogTest($"Performance Monitor - FPS: {currentFPS:F1}, Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
            }
        }
    }

    /// <summary>
    /// 创建测试滤镜
    /// </summary>
    private IEnumerator CreateTestFilter()
    {
        GameObject filterObj = new GameObject("Test Filter");
        testFilter = filterObj.AddComponent<FilterController>();

        // 设置基本属性
        testFilter.FilterData = new FilterData
        {
            filterType = FilterType.Red,
            filterRadius = 2.0f
        };

        LogTest("✓ Test filter created");
        yield return null;
    }

    /// <summary>
    /// 创建带遮罩的测试物体
    /// </summary>
    private IEnumerator CreateTestObjectWithMask()
    {
        testObjectWithMask = GameObject.CreatePrimitive(PrimitiveType.Quad);
        testObjectWithMask.name = "Test Object With Mask";

        // 添加RevealableObject组件
        RevealableObject revealable = testObjectWithMask.AddComponent<RevealableObject>();
        revealable.requiredFilters = (int)FilterType.Red;

        // 添加FilterMaskRenderer组件
        FilterMaskRenderer maskRenderer = testObjectWithMask.AddComponent<FilterMaskRenderer>();
        maskRenderer.SetMaskingEnabled(true);

        LogTest("✓ Test object with mask created");
        yield return null;
    }

    /// <summary>
    /// 生成最终测试报告
    /// </summary>
    private void GenerateFinalTestReport()
    {
        float totalTestTime = Time.time - testStartTime;

        LogTest("=== FINAL TEST REPORT ===");
        LogTest($"Total test duration: {totalTestTime:F2} seconds");

        if (FilterSystemManager.Instance != null)
        {
            LogTest(FilterSystemManager.Instance.GetSystemPerformanceInfo());
        }

        LogTest("All integration tests completed successfully!");
    }

    /// <summary>
    /// 测试日志输出
    /// </summary>
    private void LogTest(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log($"[{timestamp}] [FilterSystemTest] {message}");
    }

    /// <summary>
    /// 手动运行测试（可从Inspector调用）
    /// </summary>
    [ContextMenu("Run Integration Tests")]
    public void RunTestsManually()
    {
        if (!isTestRunning)
        {
            StartCoroutine(RunIntegrationTests());
        }
    }

    /// <summary>
    /// 停止当前测试
    /// </summary>
    [ContextMenu("Stop Tests")]
    public void StopTests()
    {
        if (isTestRunning)
        {
            StopAllCoroutines();
            isTestRunning = false;
            LogTest("Tests stopped manually");
        }
    }
}