using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    public List<Item> items = new List<Item>();
    public PlayerController player;
    public Vector3 spawnPosition;

    public static float gravity = -13f;

    [SerializeField]
    public BlockData[] blockData;

    [SerializeField]
    public Material blockMaterial;

    float maxHeight;
    float minHeight;

    public float lacunarity = 0.246f;
    public float persistence = 0.5f;
    public int octaves = 8;
    public int seed = 32;

    public int chunkWidth = 16;
    public int chunkHeight = 64;

    public int worldSizeInChunks = 20;
    public int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkWidth; }
    }

    public int viewDistanceInChunks = 5;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        //GenerateNoise(worldSizeInVoxels, worldSizeInVoxels, scale, lacunarity, persistence, octaves, seed, seedOffset);
        spawnPosition = new Vector3((worldSizeInChunks / 2f) * chunkWidth, chunkHeight + 5f, (worldSizeInChunks / 2f) * chunkWidth);

        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
    }

    private void Update()
    {
        if(items.Count == 0)
        {
            return;
        }

        for(int i  = 0; i < items.Count; i++)
        {
            items[i].TickLifeSpan(Time.deltaTime);
        }
    }
    public float Get2DPerlin(Vector2 position, float scale, int offset)
    {
        float seedVal = GenerateSeedValue(seed, offset);

        float xSample = ((position.x + 0.1f) / chunkWidth) * scale + seedVal;
        float ySample = ((position.y + 0.1f) / chunkWidth) * scale + seedVal;

        return Mathf.PerlinNoise(xSample, ySample);
    }

    /*public float Get2DPerlin(Vector2 position, float scale)
    {
        float x = position.x;
        float y = position.y;

        float val = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        float seedVal = GenerateSeedValue(seed, seedOffset);

        float xSample = ((x + 0.1f) / chunkWidth) * scale + seedVal;
        float ySample = ((y + 0.1f) / chunkWidth) * scale + seedVal;

        val = Mathf.PerlinNoise(xSample, ySample);

        for (int i = 0; i < octaves; i++)
        {
            float perlinVal = Mathf.PerlinNoise(xSample * frequency, ySample * frequency) * amplitude;
            val = val + perlinVal;

            maxValue = maxValue + amplitude;
            amplitude = amplitude * persistence;
            frequency = frequency * lacunarity;
        }

        if (val > maxHeight)
        {
            maxHeight = val;
        }
        else if(val < minHeight)
        {
            minHeight = val;
        }

        val = Mathf.InverseLerp(minHeight, maxHeight, val);

        return val;
    }*/
    public bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
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

        if(((AB + BC + AC + BA + CB + CA) / 6) > threshold)
        {
            return true;
        }

        return false;
    }
    /*public void GenerateNoise(int width, int height, float scale, float lacunarity, float persistence, int octaves, int seed, int seedOffset)
    {
        NoiseGeneration(width, height, scale, lacunarity, persistence, octaves, seed, seedOffset);
    }

    private void NoiseGeneration(int width, int height, float scale, float lacunarity, float persistence, int octaves, int seed, int seedOffset)
    {
        float[,] t = new float[width, height];
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        float seedVal = GenerateSeedValue(seed, seedOffset);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = 0;
                float frequency = 1;
                float amplitude = 1;
                float maxValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float xSample = (x * scale) + seedVal;
                    float ySample = (y * scale) + seedVal;

                    float perlinValue = Mathf.PerlinNoise(xSample * frequency, ySample * frequency) * amplitude;
                    val = val + perlinValue;

                    maxValue = maxValue + amplitude;
                    amplitude = amplitude * persistence;
                    frequency = frequency * lacunarity;
                }

                if (val > maxHeight)
                {
                    maxHeight = val;
                }
                else //if(val < minHeight)
                {
                    minHeight = val;
                }

                t[x, y] = val;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                t[x, y] = Mathf.InverseLerp(minHeight, maxHeight, t[x, y]);
            }
        }

        noiseMap = t;
    }*/

    private float GenerateSeedValue(int seed, int sample)
    {
        System.Random rand = new System.Random(seed);
        float val = rand.Next(-10000, 10000);

        return val;
    }
}
