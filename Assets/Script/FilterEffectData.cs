using System.Collections.Generic;
using UnityEngine;

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