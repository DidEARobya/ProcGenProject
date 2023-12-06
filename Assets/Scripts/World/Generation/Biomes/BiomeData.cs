using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeData", menuName = "Biomes/Biome Data")]
public class BiomeData :ScriptableObject
{
    [Header("Data")]
    public string biomeName;

    [Range(-1f, 1f)]
    public float tempMin;
    [Range(-1f, 1f)]
    public float tempMax;

    [Range(-1f, 1f)]
    public float heightMin;
    [Range(-1f, 1f)]
    public float heightMax;

    [Header("Terrain")]
    public int surfaceBlock;
    public int subSurfaceBlock;

    [Header("Vegetation")]
    public int vegetationType;

    [Range(0.1f, 1f)]
    public float vegetationZoneThreshold;
    [Range(0.1f, 1f)]
    public float vegetationPlacementThreshold;

    public bool generateVegetation = true;

    public int minSize;
    public int maxSize;

    [Header("Lodes")]
    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;

    public int minHeight;
    public int maxHeight;

    public float scale;
    public float threshold;
    public float offset;
}