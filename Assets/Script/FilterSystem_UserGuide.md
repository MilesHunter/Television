# Unity 滤镜显示系统使用指南

## 📋 目录
- [系统概述](#系统概述)
- [快速开始](#快速开始)
- [核心组件详解](#核心组件详解)
- [高级功能](#高级功能)
- [性能优化](#性能优化)
- [调试和测试](#调试和测试)
- [常见问题](#常见问题)
- [最佳实践](#最佳实践)

---

## 🎯 系统概述

Unity滤镜显示系统是一个完整的游戏机制框架，支持：
- **实时滤镜效果**：动态响应滤镜位置变化
- **精确遮罩显示**：像素级精确的物体显示控制
- **多种渲染模式**：传统模式、精确遮罩模式、混合模式
- **性能优化**：自动性能监控和优化建议
- **事件驱动架构**：完整的事件通知系统

---

## 🚀 快速开始

### 1. 基础设置

#### 步骤1：创建系统管理器
```csharp
// 在场景中创建空GameObject，命名为"FilterSystemManager"
// 添加FilterSystemManager组件
```

#### 步骤2：配置管理器
```csharp
// 在Inspector中配置以下参数：
- Enable Real Time Updates: true    // 启用实时更新
- Update Interval: 0.1f            // 更新间隔（秒）
- Is Real Time Update Enabled: true // 实时更新开关
```

#### 步骤3：创建滤镜
```csharp
// 创建GameObject，添加FilterController组件
// 配置滤镜数据：
FilterData filterData = new FilterData
{
    filterType = FilterType.Red,     // 滤镜类型
    filterRadius = 2.0f             // 影响半径
};
```

#### 步骤4：创建可显示物体
```csharp
// 在需要被滤镜控制的GameObject上添加RevealableObject组件
// 设置所需滤镜类型：
revealableObject.requiredFilters = (int)FilterType.Red;
```

### 2. 基本使用示例

```csharp
public class FilterSystemExample : MonoBehaviour
{
    public FilterController redFilter;
    public RevealableObject hiddenObject;

    void Start()
    {
        // 注册滤镜到系统
        FilterSystemManager.Instance.RegisterFilter(redFilter);

        // 注册可显示物体
        FilterSystemManager.Instance.RegisterRevealableObject(hiddenObject);

        // 启用实时更新
        FilterSystemManager.Instance.SetRealTimeUpdatesEnabled(true);
    }

    void Update()
    {
        // 移动滤镜，系统会自动更新显示效果
        if (Input.GetKey(KeyCode.W))
        {
            redFilter.transform.Translate(Vector3.up * Time.deltaTime);
        }
    }
}
```

---

## 🔧 核心组件详解

### FilterSystemManager（系统管理器）

**功能**：整个滤镜系统的核心控制器，负责协调所有组件。

**主要方法**：
```csharp
// 滤镜管理
FilterSystemManager.Instance.RegisterFilter(FilterController filter);
FilterSystemManager.Instance.UnregisterFilter(FilterController filter);

// 物体管理
FilterSystemManager.Instance.RegisterRevealableObject(RevealableObject obj);
FilterSystemManager.Instance.UnregisterRevealableObject(RevealableObject obj);

// 实时更新控制
FilterSystemManager.Instance.SetRealTimeUpdatesEnabled(bool enabled);
FilterSystemManager.Instance.SetUpdateInterval(float interval);

// 查询方法
int effect = FilterSystemManager.Instance.GetFilterEffectAtPosition(Vector2 position);
bool revealed = FilterSystemManager.Instance.IsPositionRevealed(Vector2 position, int requiredFilters);
```

**事件系统**：
```csharp
// 订阅事件
FilterSystemManager.Instance.OnFilterRegistered += OnFilterAdded;
FilterSystemManager.Instance.OnFilterEffectsUpdated += OnEffectsChanged;
FilterSystemManager.Instance.OnRevealableObjectRegistered += OnObjectAdded;

void OnFilterAdded(FilterController filter)
{
    Debug.Log($"Filter {filter.name} was added to the system");
}
```

### FilterController（滤镜控制器）

**功能**：控制单个滤镜的行为和属性。

**配置参数**：
```csharp
[Header("Filter Settings")]
public FilterData FilterData;           // 滤镜数据
public bool IsActive = true;           // 是否激活
public bool enablePositionTracking = true; // 位置跟踪

[Header("Debug")]
public bool showDebugGizmos = true;    // 显示调试图形
```

**使用示例**：
```csharp
// 动态修改滤镜属性
filterController.FilterData.filterRadius = 3.0f;
filterController.SetActive(false);

// 检查位置变化
if (filterController.HasPositionChanged())
{
    Debug.Log("Filter moved!");
    filterController.ResetPositionTracking();
}
```

### RevealableObject（可显示物体）

**功能**：可被滤镜控制显示/隐藏的物体。

**渲染模式**：
```csharp
public enum RenderMode
{
    Traditional,    // 传统模式：简单显示/隐藏
    PreciseMask,   // 精确遮罩模式：像素级控制
    Hybrid         // 混合模式：结合两种模式
}
```

**配置示例**：
```csharp
[Header("Reveal Settings")]
public int requiredFilters = 1;        // 所需滤镜类型（位掩码）
public RenderMode renderMode = RenderMode.Traditional;

[Header("Visual Settings")]
public float revealedAlpha = 1.0f;     // 显示时的透明度
public float hiddenAlpha = 0.0f;       // 隐藏时的透明度
public float transitionSpeed = 5.0f;   // 过渡速度
```

---

## 🎨 高级功能

### 1. 精确遮罩系统

精确遮罩系统提供像素级的显示控制，适用于需要精确显示效果的场景。

#### 启用精确遮罩：
```csharp
// 在RevealableObject上设置渲染模式
revealableObject.renderMode = RenderMode.PreciseMask;

// 添加FilterMaskRenderer组件
FilterMaskRenderer maskRenderer = gameObject.AddComponent<FilterMaskRenderer>();
maskRenderer.SetMaskingEnabled(true);
```

#### 配置遮罩参数：
```csharp
[Header("Mask Settings")]
public int maskResolution = 256;       // 遮罩分辨率
public float maskUpdateInterval = 0.1f; // 遮罩更新间隔
public bool enableMaskCaching = true;   // 启用遮罩缓存

[Header("Shader Settings")]
public Material maskMaterial;          // 遮罩材质
public string maskTextureProperty = "_MaskTex"; // 遮罩纹理属性名
```

#### 自定义着色器集成：
```hlsl
// 在您的着色器中添加遮罩支持
Properties
{
    _MainTex ("Texture", 2D) = "white" {}
    _MaskTex ("Mask Texture", 2D) = "white" {}
    _MaskStrength ("Mask Strength", Range(0, 1)) = 1.0
}

// 在fragment shader中：
fixed4 frag (v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    fixed4 mask = tex2D(_MaskTex, i.uv);

    // 应用遮罩
    col.a *= mask.r * _MaskStrength;

    return col;
}
```

### 2. 多滤镜组合

系统支持多个滤镜同时作用，使用位掩码进行组合：

```csharp
// 定义滤镜类型（使用位标志）
[System.Flags]
public enum FilterType
{
    None = 0,
    Red = 1,      // 0001
    Green = 2,    // 0010
    Blue = 4,     // 0100
    Yellow = 8    // 1000
}

// 设置物体需要多种滤镜
revealableObject.requiredFilters = (int)(FilterType.Red | FilterType.Blue);

// 检查滤镜组合
bool hasRedAndBlue = FilterSystemManager.Instance.IsPositionRevealed(
    position,
    (int)(FilterType.Red | FilterType.Blue)
);
```

### 3. 动态区域查询

获取指定区域内的滤镜效果分布：

```csharp
// 查询区域内的滤镜效果
Bounds queryBounds = new Bounds(center, size);
Dictionary<Vector2Int, int> effects = FilterSystemManager.Instance
    .GetFilterEffectsInBounds(queryBounds);

foreach (var kvp in effects)
{
    Vector2Int gridPos = kvp.Key;
    int filterEffect = kvp.Value;
    Debug.Log($"Position {gridPos} has filter effect: {filterEffect}");
}
```

---

## ⚡ 性能优化

### 1. 使用性能优化器

系统提供了自动性能监控和优化工具：

```csharp
// 添加性能优化器组件
FilterSystemPerformanceOptimizer optimizer = gameObject.AddComponent<FilterSystemPerformanceOptimizer>();

// 配置性能监控
optimizer.enableContinuousMonitoring = true;
optimizer.monitoringInterval = 1.0f;
optimizer.enableAutoOptimization = true;
optimizer.targetFPS = 60.0f;

// 显示性能覆盖层（按F1切换）
optimizer.showPerformanceOverlay = true;
```

### 2. 性能优化建议

#### 实时更新优化：
```csharp
// 根据需要调整更新频率
FilterSystemManager.Instance.SetUpdateInterval(0.2f); // 降低更新频率

// 在不需要时禁用实时更新
FilterSystemManager.Instance.SetRealTimeUpdatesEnabled(false);
```

#### 遮罩系统优化：
```csharp
// 降低遮罩分辨率
maskRenderer.maskResolution = 128; // 从256降到128

// 启用遮罩缓存
maskRenderer.enableMaskCaching = true;

// 增加遮罩更新间隔
maskRenderer.maskUpdateInterval = 0.2f;
```

#### 物体管理优化：
```csharp
// 使用对象池管理大量RevealableObject
public class RevealableObjectPool : MonoBehaviour
{
    private Queue<RevealableObject> pool = new Queue<RevealableObject>();

    public RevealableObject GetObject()
    {
        if (pool.Count > 0)
            return pool.Dequeue();
        else
            return CreateNewObject();
    }

    public void ReturnObject(RevealableObject obj)
    {
        obj.SetRevealed(false);
        pool.Enqueue(obj);
    }
}
```

---

## 🔍 调试和测试

### 1. 集成测试工具

使用提供的集成测试脚本验证系统功能：

```csharp
// 添加测试组件
FilterSystemIntegrationTest tester = gameObject.AddComponent<FilterSystemIntegrationTest>();

// 配置测试参数
tester.runTestsOnStart = true;
tester.enableDetailedLogging = true;
tester.testDuration = 10.0f;

// 手动运行测试
tester.RunTestsManually();
```

### 2. 调试功能

#### 可视化调试：
```csharp
// 在Scene视图中显示滤镜效果网格
// FilterSystemManager会自动绘制黄色网格显示滤镜影响区域

// 显示实时更新状态
// 绿色球体表示实时更新正在运行
```

#### 日志调试：
```csharp
// 输出系统状态
FilterSystemManager.Instance.LogSystemStatus();

// 获取性能报告
string report = performanceOptimizer.GetPerformanceReport();
Debug.Log(report);
```

### 3. 性能监控

#### 实时性能显示：
- 按 **F1** 切换性能覆盖层显示
- 显示当前FPS、内存使用、活跃滤镜数量等信息

#### 性能分析：
```csharp
// 获取详细性能信息
string perfInfo = FilterSystemManager.Instance.GetSystemPerformanceInfo();

// 检查性能等级
string grade = performanceOptimizer.GetPerformanceGrade();
// 返回: A (Excellent), B (Good), C (Fair), D (Poor), F (Critical)
```

---

## ❓ 常见问题

### Q1: 滤镜移动后物体显示没有更新？
**A**: 检查以下设置：
- 确保 `enableRealTimeUpdates` 为 true
- 检查 `updateInterval` 是否过大
- 确认滤镜的 `enablePositionTracking` 已启用

### Q2: 精确遮罩显示效果不正确？
**A**: 检查以下配置：
- 确保使用了正确的 RevealMaskShader
- 检查材质中的遮罩纹理属性名是否正确
- 确认 `maskResolution` 设置合适

### Q3: 性能问题如何解决？
**A**: 尝试以下优化：
- 增加 `updateInterval` 减少更新频率
- 降低 `maskResolution` 减少遮罩计算量
- 启用 `enableMaskCaching` 缓存遮罩
- 使用对象池管理大量物体

### Q4: 多滤镜组合不生效？
**A**: 检查位掩码设置：
```csharp
// 错误：使用加法
requiredFilters = FilterType.Red + FilterType.Blue; // ❌

// 正确：使用位或运算
requiredFilters = (int)(FilterType.Red | FilterType.Blue); // ✅
```

### Q5: 事件系统不响应？
**A**: 确保正确订阅事件：
```csharp
void Start()
{
    // 在FilterSystemManager初始化后订阅
    if (FilterSystemManager.Instance != null)
    {
        FilterSystemManager.Instance.OnFilterEffectsUpdated += OnEffectsChanged;
    }
}

void OnDestroy()
{
    // 记得取消订阅避免内存泄漏
    if (FilterSystemManager.Instance != null)
    {
        FilterSystemManager.Instance.OnFilterEffectsUpdated -= OnEffectsChanged;
    }
}
```

---

## 💡 最佳实践

### 1. 系统架构建议

```csharp
// 推荐的项目结构
FilterSystem/
├── Core/
│   ├── FilterSystemManager.cs
│   ├── FilterController.cs
│   └── RevealableObject.cs
├── Rendering/
│   ├── FilterMaskRenderer.cs
│   ├── MaskTextureGenerator.cs
│   └── Shaders/
│       └── RevealMaskShader.shader
├── Testing/
│   ├── FilterSystemIntegrationTest.cs
│   └── FilterSystemPerformanceOptimizer.cs
└── Examples/
    └── FilterSystemExample.cs
```

### 2. 性能优化策略

#### 分层优化：
1. **系统级**：合理设置更新频率和缓存策略
2. **组件级**：优化单个组件的计算复杂度
3. **渲染级**：使用高效的着色器和纹理格式

#### 内存管理：
```csharp
// 使用对象池避免频繁创建销毁
public class FilterSystemObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 100;
    }

    public PoolConfig[] poolConfigs;
    private Dictionary<string, Queue<GameObject>> pools;

    void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        pools = new Dictionary<string, Queue<GameObject>>();

        foreach (var config in poolConfigs)
        {
            var pool = new Queue<GameObject>();

            for (int i = 0; i < config.initialSize; i++)
            {
                var obj = Instantiate(config.prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            pools[config.prefab.name] = pool;
        }
    }
}
```

### 3. 代码组织建议

#### 使用接口提高可扩展性：
```csharp
public interface IFilterEffect
{
    void ApplyEffect(Vector2 position, float intensity);
    void RemoveEffect(Vector2 position);
}

public interface IRevealable
{
    bool ShouldReveal(int filterMask);
    void SetRevealed(bool revealed);
}
```

#### 使用事件解耦组件：
```csharp
public static class FilterSystemEvents
{
    public static event System.Action<FilterController> OnFilterAdded;
    public static event System.Action<FilterController> OnFilterRemoved;
    public static event System.Action OnSystemUpdated;

    public static void TriggerFilterAdded(FilterController filter)
    {
        OnFilterAdded?.Invoke(filter);
    }
}
```

### 4. 调试和维护

#### 添加详细日志：
```csharp
public static class FilterSystemLogger
{
    public static bool EnableDebugLogs = true;

    public static void Log(string message, LogType type = LogType.Log)
    {
        if (!EnableDebugLogs) return;

        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        string formattedMessage = $"[{timestamp}] [FilterSystem] {message}";

        switch (type)
        {
            case LogType.Warning:
                Debug.LogWarning(formattedMessage);
                break;
            case LogType.Error:
                Debug.LogError(formattedMessage);
                break;
            default:
                Debug.Log(formattedMessage);
                break;
        }
    }
}
```

---

## 📚 总结

Unity滤镜显示系统提供了完整的滤镜管理解决方案，支持：

- ✅ **实时响应**：动态更新显示效果
- ✅ **精确控制**：像素级显示精度
- ✅ **高性能**：自动优化和监控
- ✅ **易扩展**：模块化架构设计
- ✅ **易调试**：丰富的调试工具

通过合理配置和使用本系统，您可以轻松实现复杂的滤镜显示效果，为游戏增添丰富的视觉体验。

---

**🔗 相关文件**：
- `FilterSystemManager.cs` - 核心管理器
- `FilterController.cs` - 滤镜控制器
- `RevealableObject.cs` - 可显示物体
- `FilterMaskRenderer.cs` - 遮罩渲染器
- `FilterSystemIntegrationTest.cs` - 集成测试
- `FilterSystemPerformanceOptimizer.cs` - 性能优化器

**📧 技术支持**：如有问题，请查看代码注释或运行集成测试进行诊断。