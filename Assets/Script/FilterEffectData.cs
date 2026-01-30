using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FilterType
{
    Red = 1,
    Blue = 2,
    Green = 4,
    Yellow = 8,
    Purple = 16,
    // 可以继续添加，使用2的幂次方便于位运算
}

[CreateAssetMenu(fileName = "FilterEffect", menuName = "Game/FilterEffect")]
public class FilterEffectData : ScriptableObject
{
    [Header("Filter Properties")]
    public FilterType filterType;
    public Color filterColor = Color.white;
    public float filterRadius = 3f;

    [Header("Visual Effects")]
    public GameObject filterVisualPrefab;
    public ParticleSystem filterParticles;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip activateSound;
}