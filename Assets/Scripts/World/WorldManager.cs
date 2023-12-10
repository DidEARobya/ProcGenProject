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
                    worldSettings.scale,
                    worldSettings.lacunarity,
                    worldSettings.persistence,
                    worldSettings.octaves,
                    worldSettings.seed,
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
                    0.1f,
                    2,
                    0.5f,
                    4,
                    24,
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
    public bool enableThreading;

    public float scale;
    public float lacunarity;
    public float persistence;
    public int octaves;
    public int seed;

    public int chunkWidth;
    public int chunkHeight;

    public int worldSizeInChunks;
    public int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkWidth; }
    }

    public int loadDistance;
    public int viewDistanceInChunks;

    public int seaLevel = 70;
    public WorldData(bool _enableThreading, float _scale, float _lacunarity, float _persistence, int _octaves, int _seed, int _chunkWidth, int _chunkHeight, int _worldSizeInChunks, int _loadDistance, int _viewDistance)
    {
        enableThreading = _enableThreading;

        scale = _scale;
        lacunarity = _lacunarity;
        persistence = _persistence;
        octaves = _octaves;

        seed = _seed;

        chunkWidth = _chunkWidth;
        chunkHeight = _chunkHeight;
        worldSizeInChunks = _worldSizeInChunks;

        loadDistance = _loadDistance;
        viewDistanceInChunks = _viewDistance;
    }
}