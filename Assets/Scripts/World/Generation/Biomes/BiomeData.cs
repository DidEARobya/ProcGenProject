using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeData", menuName = "Biomes/Biome Data")]
public class BiomeData :ScriptableObject
{

    public string biomeName;

    [Header("Terrain")]
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    [Header("Forest")]
    public float treeZoneScale;
    [Range(0.1f, 1f)]
    public float treeZoneThreshold;

    public float treePlacementScale;
    [Range(0.1f, 1f)]
    public float treePlacementThreshold;

    public int minTreeSize;
    public int maxTreeSize;

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