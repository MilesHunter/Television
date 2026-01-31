using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遮罩纹理生成器 - 根据滤镜效果生成精确的遮罩纹理
/// </summary>
public static class MaskTextureGenerator
{
    // 纹理池，避免频繁创建和销毁纹理
    private static Dictionary<string, Texture2D> texturePool = new Dictionary<string, Texture2D>();
    private static int maxPoolSize = 50;

    /// <summary>
    /// 为指定的RevealableObject生成遮罩纹理
    /// </summary>
    /// <param name="obj">需要生成遮罩的物体</param>
    /// <param name="activeFilters">当前激活的滤镜列表</param>
    /// <param name="resolution">遮罩纹理分辨率</param>
    /// <returns>生成的遮罩纹理</returns>
    public static Texture2D GenerateMaskTexture(RevealableObject obj, List<FilterController> activeFilters, int resolution = 256)
    {
        if (obj == null || activeFilters == null)
        {
            return GetBlackTexture(resolution);
        }

        // 获取物体边界
        Bounds objBounds = GetObjectBounds(obj);
        if (objBounds.size.magnitude < 0.01f)
        {
            return GetBlackTexture(resolution);
        }

        // 生成缓存键
        string cacheKey = GenerateCacheKey(obj, activeFilters, resolution, objBounds);

        // 检查缓存
        if (texturePool.ContainsKey(cacheKey))
        {
            return texturePool[cacheKey];
        }

        // 生成新的遮罩纹理
        Texture2D maskTexture = CreateMaskTexture(obj, activeFilters, resolution, objBounds);

        // 添加到缓存池
        AddToTexturePool(cacheKey, maskTexture);

        return maskTexture;
    }

    /// <summary>
    /// 创建遮罩纹理的核心逻辑
    /// </summary>
    private static Texture2D CreateMaskTexture(RevealableObject obj, List<FilterController> activeFilters, int resolution, Bounds objBounds)
    {
        Texture2D maskTexture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
        maskTexture.filterMode = FilterMode.Bilinear;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        // 预计算边界信息
        Vector2 boundsMin = new Vector2(objBounds.min.x, objBounds.min.y);
        Vector2 boundsSize = new Vector2(objBounds.size.x, objBounds.size.y);
        float pixelSizeX = boundsSize.x / resolution;
        float pixelSizeY = boundsSize.y / resolution;

        // 生成遮罩数据
        Color[] pixels = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // 计算世界坐标
                Vector2 worldPos = new Vector2(
                    boundsMin.x + (x + 0.5f) * pixelSizeX,
                    boundsMin.y + (y + 0.5f) * pixelSizeY
                );

                // 计算该像素的滤镜覆盖程度
                float coverage = CalculateFilterCoverage(worldPos, activeFilters, obj.RequiredFilters, obj.RequireAllFilters);

                // 存储到像素数组
                int pixelIndex = y * resolution + x;
                pixels[pixelIndex] = new Color(coverage, coverage, coverage, 1f);
            }
        }

        // 应用像素数据
        maskTexture.SetPixels(pixels);
        maskTexture.Apply();

        return maskTexture;
    }

    /// <summary>
    /// 计算指定位置的滤镜覆盖程度
    /// </summary>
    /// <param name="worldPos">世界坐标位置</param>
    /// <param name="activeFilters">激活的滤镜列表</param>
    /// <param name="requiredFilters">需要的滤镜类型</param>
    /// <param name="requireAllFilters">是否需要所有滤镜</param>
    /// <returns>覆盖程度 (0-1)</returns>
    private static float CalculateFilterCoverage(Vector2 worldPos, List<FilterController> activeFilters, int requiredFilters, bool requireAllFilters)
    {
        if (requiredFilters == 0 || activeFilters.Count == 0)
        {
            return 0f;
        }

        int currentEffect = 0;
        float maxCoverage = 0f;

        // 计算每个滤镜的影响
        foreach (FilterController filter in activeFilters)
        {
            if (!filter.IsActive || filter.FilterData == null)
                continue;

            Vector2 filterPos = filter.transform.position;
            float distance = Vector2.Distance(worldPos, filterPos);
            float radius = filter.FilterData.filterRadius;

            if (distance <= radius)
            {
                // 计算覆盖强度（距离越近强度越高）
                float coverage = 1f - (distance / radius);
                coverage = Mathf.Clamp01(coverage);

                // 应用滤镜效果到当前效果值
                int filterValue = (int)filter.FilterData.filterType;
                currentEffect |= filterValue;

                // 记录最大覆盖程度
                maxCoverage = Mathf.Max(maxCoverage, coverage);
            }
        }

        // 检查是否满足滤镜需求
        bool meetsRequirement;
        if (requireAllFilters)
        {
            // AND逻辑：需要所有指定的滤镜
            meetsRequirement = (currentEffect & requiredFilters) == requiredFilters;
        }
        else
        {
            // OR逻辑：需要任意一个指定的滤镜
            meetsRequirement = (currentEffect & requiredFilters) != 0;
        }

        return meetsRequirement ? maxCoverage : 0f;
    }

    /// <summary>
    /// 获取物体的边界框
    /// </summary>
    private static Bounds GetObjectBounds(RevealableObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        // 默认边界
        Vector3 pos = obj.transform.position;
        return new Bounds(pos, Vector3.one);
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    private static string GenerateCacheKey(RevealableObject obj, List<FilterController> activeFilters, int resolution, Bounds bounds)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 物体信息
        sb.Append($"obj_{obj.GetInstanceID()}");
        sb.Append($"_req_{obj.RequiredFilters}");
        sb.Append($"_all_{obj.RequireAllFilters}");
        sb.Append($"_res_{resolution}");
        sb.Append($"_bounds_{bounds.center.x:F2}_{bounds.center.y:F2}_{bounds.size.x:F2}_{bounds.size.y:F2}");

        // 滤镜信息
        sb.Append("_filters");
        foreach (FilterController filter in activeFilters)
        {
            if (filter != null && filter.IsActive && filter.FilterData != null)
            {
                Vector3 pos = filter.transform.position;
                sb.Append($"_{(int)filter.FilterData.filterType}_{pos.x:F2}_{pos.y:F2}_{filter.FilterData.filterRadius:F2}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 添加纹理到缓存池
    /// </summary>
    private static void AddToTexturePool(string key, Texture2D texture)
    {
        // 如果池已满，移除最旧的纹理
        if (texturePool.Count >= maxPoolSize)
        {
            var oldestKey = "";
            foreach (var kvp in texturePool)
            {
                oldestKey = kvp.Key;
                break; // 获取第一个（最旧的）
            }

            if (!string.IsNullOrEmpty(oldestKey))
            {
                if (texturePool[oldestKey] != null)
                {
                    Object.DestroyImmediate(texturePool[oldestKey]);
                }
                texturePool.Remove(oldestKey);
            }
        }

        texturePool[key] = texture;
    }

    /// <summary>
    /// 获取黑色纹理（完全隐藏）
    /// </summary>
    private static Texture2D GetBlackTexture(int resolution)
    {
        string key = $"black_{resolution}";

        if (!texturePool.ContainsKey(key))
        {
            Texture2D blackTexture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
            Color[] pixels = new Color[resolution * resolution];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.black;
            }

            blackTexture.SetPixels(pixels);
            blackTexture.Apply();

            texturePool[key] = blackTexture;
        }

        return texturePool[key];
    }

    /// <summary>
    /// 获取白色纹理（完全显示）
    /// </summary>
    public static Texture2D GetWhiteTexture(int resolution)
    {
        string key = $"white_{resolution}";

        if (!texturePool.ContainsKey(key))
        {
            Texture2D whiteTexture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
            Color[] pixels = new Color[resolution * resolution];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            whiteTexture.SetPixels(pixels);
            whiteTexture.Apply();

            texturePool[key] = whiteTexture;
        }

        return texturePool[key];
    }

    /// <summary>
    /// 清理纹理池
    /// </summary>
    public static void ClearTexturePool()
    {
        foreach (var kvp in texturePool)
        {
            if (kvp.Value != null)
            {
                Object.DestroyImmediate(kvp.Value);
            }
        }
        texturePool.Clear();
    }

    /// <summary>
    /// 获取纹理池状态信息
    /// </summary>
    public static string GetPoolStatus()
    {
        return $"Texture Pool: {texturePool.Count}/{maxPoolSize} textures cached";
    }

    /// <summary>
    /// 设置纹理池最大大小
    /// </summary>
    public static void SetMaxPoolSize(int size)
    {
        maxPoolSize = Mathf.Max(1, size);

        // 如果当前池大小超过新的最大值，清理多余的纹理
        while (texturePool.Count > maxPoolSize)
        {
            var oldestKey = "";
            foreach (var kvp in texturePool)
            {
                oldestKey = kvp.Key;
                break;
            }

            if (!string.IsNullOrEmpty(oldestKey))
            {
                if (texturePool[oldestKey] != null)
                {
                    Object.DestroyImmediate(texturePool[oldestKey]);
                }
                texturePool.Remove(oldestKey);
            }
        }
    }
}