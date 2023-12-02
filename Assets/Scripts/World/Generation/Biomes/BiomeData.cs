using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeData", menuName = "Biomes/Biome Data")]
public class BiomeData :ScriptableObject
{
    [Header("Data")]
    public string biomeName;
    public int offset;
    public float scale;

    [Header("Terrain")]
    public int terrainHeight;
    public float terrainScale;

    public int surfaceBlock;
    public int subSurfaceBlock;

    [Header("Vegetation")]
    public int vegetationType;

    public float vegetationZoneScale;
    [Range(0.1f, 1f)]
    public float vegetationZoneThreshold;

    public float vegetationPlacementScale;
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