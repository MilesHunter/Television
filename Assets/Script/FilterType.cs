using UnityEngine;

/// <summary>
/// 显示渲染模式枚举
/// </summary>
public enum RevealRenderingMode
{
    Transparency,   // 传统透明度模式（向后兼容）
    PreciseMask     // 精确遮罩模式（新功能）
}

/// <summary>
/// 滤镜类型枚举 - 使用位标志支持多滤镜组合
/// </summary>
[System.Flags]
public enum FilterType
{
    None = 0,
    Red = 1,
    Blue = 2,
    Green = 4,
    Yellow = 8,
    Purple = 16,
    Orange = 32,
    Cyan = 64,
    Magenta = 128
}