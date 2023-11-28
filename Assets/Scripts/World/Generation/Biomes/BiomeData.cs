using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeData", menuName = "Biomes/Biome Data")]
public class BiomeData :ScriptableObject
{
    [SerializeField]
    public string biomeName;

    [SerializeField]
    public int solidGroundHeight;
    [SerializeField]
    public int terrainHeight;
    [SerializeField]
    public float terrainScale;

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