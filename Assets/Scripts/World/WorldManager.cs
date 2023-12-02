using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    private WorldSettings worldSettings;

    public bool enableThreading;

    public List<Item> items = new List<Item>();

    public PlayerController player;
    public Vector3 spawnTemp;
    public Vector3 spawnPosition;

    public float gravity = -13f;

    public float[,] noiseMap;

    [SerializeField]
    public BlockData[] blockData;

    [SerializeField]
    public Material blockMaterial;
    [SerializeField]
    public Material transparentBlockMaterial;

    float maxHeight;
    float minHeight;

    public float scale = 0.1f;
    public float lacunarity = 0.246f;
    public float persistence = 0.5f;
    public int octaves = 8;
    public int seed = 32;
    public int seedOffset = 0;

    public int chunkWidth = 16;
    public int chunkHeight = 64;

    private int width;
    private int height;

    public int worldSizeInChunks = 20;
    public int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkWidth; }
    }

    public int loadDistance = 10;
    public int viewDistanceInChunks = 5;
    public bool isSpawned;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            worldSettings = GameObject.Find("WorldSettings")?.GetComponent<WorldSettings>();

            if(worldSettings != null)
            {
                enableThreading = worldSettings.enableThreading;
                seed = worldSettings.seed;
                seedOffset = worldSettings.seedOffset;

                worldSizeInChunks = worldSettings.worldSizeInChunks;
                chunkWidth = worldSettings.chunkWidth;
                chunkHeight = worldSettings.chunkHeight;

                viewDistanceInChunks = worldSettings.viewDistanceInChunks;
            }

            spawnTemp = new Vector3((worldSizeInChunks / 2f) * chunkWidth, chunkHeight - 20f, (worldSizeInChunks / 2f) * chunkWidth);
            spawnPosition = spawnTemp;

            maxHeight = float.MinValue;
            minHeight = float.MaxValue;

            width = worldSizeInVoxels;
            height = worldSizeInChunks;
        }
    }

    private void Update()
    {
        if(isSpawned == false && spawnTemp != spawnPosition)
        {
            SpawnPlayer(spawnPosition);
            isSpawned = true;
        }

        if(items.Count > 0)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].TickLifeSpan(Time.deltaTime);
            }
        }
    }
    public void SpawnPlayer(Vector3 position)
    {
        player.transform.position = position;
    }

    public float GenerateSeedValue()
    {
        System.Random rand = new System.Random(seed);
        float val = rand.Next(-10000, 10000);

        return val;
    }
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
       else// if(val < minHeight)
       {
           minHeight = val;
       }

       val = Mathf.InverseLerp(minHeight, maxHeight, val);

       return val;
   }*/

/*private void NoiseGeneration(int width, int height, float scale, float lacunarity, float persistence, int octaves, int seed, int seedOffset)
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
            else
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