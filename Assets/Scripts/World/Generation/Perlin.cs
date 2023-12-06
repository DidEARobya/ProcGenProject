using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class Perlin
{
    public static float GetHeightMapPerlin(Vector2Int position, float scale)
    {
        float seedVal = WorldManager.instance.GenerateSeedValue();

        int octaves = WorldManager.instance.worldData.octaves;
        float persistence = WorldManager.instance.worldData.persistence;
        float lacunarity = WorldManager.instance.worldData.lacunarity;

        float noise = 0;
        float amplitude = 1;
        float frequency = 1;
        for (int i = 0; i < octaves; i++)
        {
            float xSample = ((position.x + 0.1f) / WorldManager.instance.worldData.chunkWidth) * scale * frequency + seedVal;
            float ySample = ((position.y + 0.1f) / WorldManager.instance.worldData.chunkWidth) * scale * frequency + seedVal;

            float perlinValue = Mathf.PerlinNoise(xSample, ySample) * 2 - 1;
            noise += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noise;
    }

    public static float Get2DPerlin(Vector2Int position, float scale, float offset)
    {
        float seedVal = WorldManager.instance.GenerateSeedValue();

        float xSample = ((position.x + 0.1f) / WorldManager.instance.worldData.chunkWidth) * scale + seedVal + offset;
        float ySample = ((position.y + 0.1f) / WorldManager.instance.worldData.chunkWidth) * scale + seedVal + offset;

        return Mathf.PerlinNoise(xSample, ySample);
    }
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if (((AB + BC + AC + BA + CB + CA) / 6) > threshold)
        {
            return true;
        }

        return false;
    }
}

/*public static float GetContinentalness(Vector2Int position)
{
    float worldSize = WorldManager.instance.worldData.worldSizeInVoxels;

    float mapHalfWorld = worldSize / 2;

    float x = position.x - mapHalfWorld;
    float y = position.y - mapHalfWorld;

    float distance = new Vector2(x, y).magnitude;

    float percentage = (distance / worldSize);
    percentage = Mathf.Clamp(percentage, 0, 1);

    return Mathf.Lerp(1, -1, percentage);
}

public static float GetPV(Vector2Int position, float noise)
{
    float highVal;
    float lowVal;
    float val;
    float total;
    float percentage;

    if (noise < -0.6f)
    {
        lowVal = Mathf.Abs(-1);
        highVal = Mathf.Abs(-0.6f);
        val = Mathf.Abs(noise);

        total = lowVal + highVal;

        percentage = val / total;

        return Mathf.Lerp(lowVal, highVal, percentage);
    }

    if (noise < 0.75f)
    {
        lowVal = Mathf.Abs(-0.6f);
        highVal = 0.4f;
        val = Mathf.Abs(noise);

        total = lowVal + highVal;

        percentage = val / total;

        return Mathf.Lerp(lowVal, highVal, percentage);

    }

    lowVal = 0.4f;
    highVal = 1f;
    val = Mathf.Abs(noise);

    total = lowVal + highVal;

    percentage = val / total;

    return Mathf.Lerp(lowVal, highVal, percentage);
}*/
