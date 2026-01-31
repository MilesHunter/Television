using System.Collections;
using UnityEngine;

/// <summary>
/// 测试滤镜系统实时更新功能的辅助脚本
/// </summary>
public class FilterSystemTester : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableAutoTest = false;
    public float testDuration = 10f;
    public float moveSpeed = 2f;
    public Vector2 moveRange = new Vector2(5f, 5f);

    [Header("Test Targets")]
    public FilterController testFilter;
    public RevealableObject testObject;

    [Header("Debug Info")]
    public bool showDebugInfo = true;

    private Vector3 originalFilterPosition;
    private bool isTestRunning = false;
    private float testStartTime;
    private int updateCount = 0;
    private int lastEffectValue = -1;

    void Start()
    {
        if (testFilter != null)
        {
            originalFilterPosition = testFilter.transform.position;
        }

        if (enableAutoTest)
        {
            StartCoroutine(RunAutoTest());
        }
    }

    void Update()
    {
        if (showDebugInfo && testObject != null && FilterSystemManager.Instance != null)
        {
            // 检查物体位置的滤镜效果
            int currentEffect = FilterSystemManager.Instance.GetFilterEffectAtPosition(testObject.transform.position);

            if (currentEffect != lastEffectValue)
            {
                updateCount++;
                lastEffectValue = currentEffect;

                Debug.Log($"[FilterSystemTester] Effect changed at object position: {currentEffect} (Update #{updateCount})");
            }
        }

        // 手动测试控制
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartManualTest();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetFilterPosition();
        }
    }

    [ContextMenu("Start Manual Test")]
    public void StartManualTest()
    {
        if (!isTestRunning)
        {
            StartCoroutine(RunManualTest());
        }
    }

    [ContextMenu("Reset Filter Position")]
    public void ResetFilterPosition()
    {
        if (testFilter != null)
        {
            testFilter.transform.position = originalFilterPosition;
            Debug.Log("[FilterSystemTester] Filter position reset");
        }
    }

    private IEnumerator RunAutoTest()
    {
        Debug.Log("[FilterSystemTester] Starting automatic test...");
        isTestRunning = true;
        testStartTime = Time.time;
        updateCount = 0;

        while (Time.time - testStartTime < testDuration)
        {
            if (testFilter != null)
            {
                // 让滤镜做圆周运动
                float angle = (Time.time - testStartTime) * moveSpeed;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * moveRange.x,
                    Mathf.Sin(angle) * moveRange.y,
                    0
                );
                testFilter.transform.position = originalFilterPosition + offset;
            }

            yield return new WaitForSeconds(0.05f); // 更频繁地移动以测试实时更新
        }

        ResetFilterPosition();
        isTestRunning = false;

        Debug.Log($"[FilterSystemTester] Auto test completed. Total updates detected: {updateCount}");
    }

    private IEnumerator RunManualTest()
    {
        Debug.Log("[FilterSystemTester] Starting manual test (Press T to start, R to reset)...");
        isTestRunning = true;
        testStartTime = Time.time;
        updateCount = 0;

        // 简单的来回移动测试
        Vector3 startPos = originalFilterPosition;
        Vector3 endPos = startPos + new Vector3(moveRange.x, moveRange.y, 0);

        float testTime = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < testTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.PingPong(elapsedTime * moveSpeed, 1f);

            if (testFilter != null)
            {
                testFilter.transform.position = Vector3.Lerp(startPos, endPos, t);
            }

            yield return null;
        }

        ResetFilterPosition();
        isTestRunning = false;

        Debug.Log($"[FilterSystemTester] Manual test completed. Total updates detected: {updateCount}");
    }

    void OnDrawGizmos()
    {
        if (testFilter != null)
        {
            // 绘制移动范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(originalFilterPosition, new Vector3(moveRange.x * 2, moveRange.y * 2, 0));

            // 绘制当前位置到原始位置的连线
            Gizmos.color = Color.red;
            Gizmos.DrawLine(originalFilterPosition, testFilter.transform.position);
        }

        if (testObject != null)
        {
            // 绘制测试物体
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(testObject.transform.position, 0.5f);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Filter System Tester", GUI.skin.box);

        GUILayout.Label($"Test Running: {isTestRunning}");
        GUILayout.Label($"Updates Detected: {updateCount}");
        GUILayout.Label($"Last Effect Value: {lastEffectValue}");

        if (FilterSystemManager.Instance != null)
        {
            GUILayout.Label($"Update Interval: {FilterSystemManager.Instance.updateInterval:F2}s");
            GUILayout.Label($"Real-time Updates: {FilterSystemManager.Instance.enableRealTimeUpdates}");
        }

        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("T - Start Manual Test");
        GUILayout.Label("R - Reset Filter Position");

        GUILayout.EndArea();
    }
}