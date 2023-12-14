using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class Perlin
{
    public static float GetHeightMapPerlin(Vector2Int position, float scale, float offset)
    {
        WorldData worldData = WorldManager.instance.worldData;

        int octaves = worldData.octaves;
        float persistence = worldData.persistence;
        float lacunarity = worldData.lacunarity;

        float noise = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float xSample = ((position.x + 0.1f) / worldData.chunkWidth) * scale * frequency + offset;
            float ySample = ((position.y + 0.1f) / worldData.chunkWidth) * scale * frequency + offset;

            float perlinValue = Mathf.PerlinNoise(xSample, ySample) * 2 - 1;
            noise += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noise;
    }


    public static float GetTemperatureNoise(Vector2Int position, float offset)
    {
        WorldData worldData = WorldManager.instance.worldData;

        int octaves = worldData.octaves;
        float persistence = worldData.persistence;
        float lacunarity = worldData.lacunarity;

        float noise = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float xSample = ((position.x + 0.1f) / worldData.chunkWidth) * worldData.scale * frequency + 10000 + offset;
            float ySample = ((position.y + 0.1f) / worldData.chunkWidth) * worldData.scale * frequency + 10000 + offset;

            float perlinValue = Mathf.PerlinNoise(xSample, ySample) * 2 - 1;
            noise += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noise;

        /*WorldData worldData = WorldManager.instance.worldData;

        float xSample = ((position.x + 0.1f) / worldData.chunkWidth) * worldData.scale + 10000 + offset;
        float ySample = ((position.y + 0.1f) / worldData.chunkWidth) * worldData.scale + 10000 + offset;

        return Mathf.PerlinNoise(xSample, ySample) * 2 - 1;*/

    }

    public static int GetBiomeIndex(Vector2Int position, BiomeData[] biomes, float heightNoise, float offset)
    {
        int biomeIndex = 0;
        float tempNoise = GetTemperatureNoise(position, offset);

        for (int i = 0; i < biomes.Length; i++)
        {
            if (heightNoise > biomes[i].heightMin && heightNoise < biomes[i].heightMax)
            {
                if (tempNoise > biomes[i].tempMin && tempNoise < biomes[i].tempMax)
                {
                    biomeIndex = i;
                }
            }
        }

        return biomeIndex;
    }

    public static float GetVegetationZoneNoise(Vector2Int position, float offset, float scale)
    {
        WorldData worldData = WorldManager.instance.worldData;

        float xSample = (((position.x + 0.1f) / worldData.chunkWidth) * scale) + 5000 + offset;
        float ySample = (((position.y + 0.1f) / worldData.chunkWidth) * scale) + 5000 + offset;

        return Mathf.PerlinNoise(xSample, ySample);
    }
    public static float GetVegetationDensityNoise(Vector2Int position, float offset, float scale)
    {
        WorldData worldData = WorldManager.instance.worldData;

        float xSample = (((position.x + 0.1f) / worldData.chunkWidth) * scale) + 2500 + offset;
        float ySample = (((position.y + 0.1f) / worldData.chunkWidth) * scale) + 2500 + offset;

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
