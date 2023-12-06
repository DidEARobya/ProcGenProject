using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;
    private WorldSettings worldSettings;
    public WorldData worldData;

    public List<Item> items = new List<Item>();

    public PlayerController player;
    public Vector3Int spawnTemp;
    public Vector3Int   spawnPosition;

    public float gravity = -13f;

    [SerializeField]
    public BlockData[] blockData;

    [SerializeField]
    public Material blockMaterial;
    [SerializeField]
    public Material transparentBlockMaterial;

    public bool isSpawned;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            worldSettings = GameObject.Find("WorldSettings")?.GetComponent<WorldSettings>();

            if (worldSettings != null)
            {
                worldData = new WorldData(
                    worldSettings.enableThreading,
                    worldSettings.enableThreading,
                    worldSettings.scale,
                    worldSettings.lacunarity,
                    worldSettings.persistence,
                    worldSettings.octaves,
                    worldSettings.seed,
                    worldSettings.seedOffset,
                    worldSettings.chunkWidth,
                    worldSettings.chunkHeight,
                    worldSettings.worldSizeInChunks,
                    worldSettings.loadDistance,
                    worldSettings.viewDistanceInChunks);
            }
            else
            {
                worldData = new WorldData(
                    true,
                    true,
                    0.1f,
                    2,
                    0.5f,
                    4,
                    0,
                    0,
                    16,
                    128,
                    30,
                    10,
                    5);
            }

            spawnTemp = Vector3Int.FloorToInt(new Vector3((worldData.worldSizeInChunks / 2f) * worldData.chunkWidth, worldData.chunkHeight - 20f, (worldData.worldSizeInChunks / 2f) * worldData.chunkWidth));
            spawnPosition = spawnTemp;
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
    public void SpawnPlayer(Vector3Int position)
    {
        player.transform.position = position;
    }

    public float GenerateSeedValue()
    {
        System.Random rand = new System.Random(worldData.seed);
        float val = rand.Next(-10000, 10000);

        return val;
    }
}

public class WorldData
{
    public bool extremeTerrain;
    public bool enableThreading;

    public float scale = 0.01f;
    public float lacunarity = 2.1f;
    public float persistence = 0.5f;
    public int octaves = 4;
    public int seed = 32;
    public int seedOffset = 0;

    public int chunkWidth = 16;
    public int chunkHeight = 64;

    public int worldSizeInChunks = 20;
    public int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkWidth; }
    }

    public int loadDistance = 10;
    public int viewDistanceInChunks = 5;

    public int seaLevel = 70;
    public WorldData(bool _extremeTerrain, bool _enableThreading, float _scale, float _lacunarity, float _persistence, int _octaves, int _seed, int _seedOffset, int _chunkWidth, int _chunkHeight, int _worldSizeInChunks, int _loadDistance, int _viewDistance)
    {
        extremeTerrain = _extremeTerrain;
        enableThreading = _enableThreading;

        scale = _scale;
        lacunarity = _lacunarity;
        persistence = _persistence;
        octaves = _octaves;

        seed = _seed;
        seedOffset = _seedOffset;

        chunkWidth = _chunkWidth;
        chunkHeight = _chunkHeight;
        worldSizeInChunks = _worldSizeInChunks;

        loadDistance = _loadDistance;
        viewDistanceInChunks = _viewDistance;
    }
}