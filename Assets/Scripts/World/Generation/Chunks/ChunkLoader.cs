using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;


public class ChunkLoader : MonoBehaviour
{
    public static ChunkLoader instance;
    public PoissonDiscSampler sampler;

    Transform player;

    [SerializeField]
    public BiomeData[] biomes;

    WorldManager worldManager;
    WorldData worldData;
    float offset;

    protected float[,] noiseMap;

    protected int chunkCount;
    protected int voxelCount;

    protected int chunkWidth;
    protected int chunkHeight;

    protected int loadDistance;
    protected int viewDistance;

    Chunk[,] chunks;
    List<ChunkVector> activeChunks = new List<ChunkVector>();

    public ChunkVector currentChunk;
    ChunkVector lastChunk;

    public Queue<Chunk> toDraw = new Queue<Chunk>();
    public List<Chunk> toUpdate = new List<Chunk>();
    public List<Chunk> toLoad = new List<Chunk>();

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    private bool applyingMods = false;

    Thread chunkUpdateThread;
    public object updateThreadLock = new object();

    Thread chunkLoadThread;
    public object loadThreadLock = new object();

    private int loadIndex = 0;
    public bool isReady = false;

    private void Start()
    {
        if(instance == null)
        { 
            instance = this;

            worldManager = WorldManager.instance;
            worldData = worldManager.worldData;
            player = worldManager.player.transform;

            offset = WorldManager.instance.GenerateSeedValue();

            chunkCount = worldData.worldSizeInChunks;
            voxelCount = worldData.worldSizeInVoxels;

            chunkWidth = worldData.chunkWidth;
            chunkHeight = worldData.chunkHeight;

            loadDistance = worldData.loadDistance;
            viewDistance = worldData.viewDistanceInChunks;

            loadIndex = (viewDistance * 2) * (viewDistance * 2);

            chunks = new Chunk[chunkCount, chunkCount];

            if (worldData.enableThreading == true)
            {
                chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
                chunkUpdateThread.Start();

                chunkLoadThread = new Thread(new ThreadStart(ThreadedLoad));
                chunkLoadThread.Start();
            }

            GenerateSpawnChunks();
        }
    }

    private void Update()
    {
        currentChunk = GetChunkVectorFromVector3(player.position);

        if (currentChunk.Equals(lastChunk) == false)
        {
            CheckLoadDistance();
            CheckViewDistance();
            lastChunk = currentChunk;
        }

        if (toDraw.Count > 0)
        {
            if(loadIndex > 0)
            {
                loadIndex -= 1;

                if(loadIndex == 0)
                {
                    isReady = true;
                }
            }

            toDraw.Dequeue().CreateMesh();
        }

        if (worldData.enableThreading == false)
        {
            if (toLoad.Count > 0)
            {
                LoadChunk();
            }

            if (applyingMods == false)
            {
                ApplyModifications();
            }

            if (toUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    protected void GenerateSpawnChunks()
    {
        int chunksCount = Mathf.FloorToInt(chunkCount / 2);

        for(int x = chunksCount - viewDistance; x < chunksCount + viewDistance; x++)
        {
            for (int z = chunksCount - viewDistance; z < chunksCount + viewDistance; z++)
            {
                ChunkVector vector = new ChunkVector(x, z);
                chunks[x, z] = new Chunk(vector, chunkWidth, chunkHeight);
            }
        }

        player.position = WorldManager.instance.spawnPosition;
        lastChunk = currentChunk;

        CheckViewDistance();
    }

    private void UpdateChunks()
    {
        lock (updateThreadLock)
        {
            if (toUpdate[0] == null)
            {
                return;
            }

            toUpdate[0].UpdateChunk();

            if (activeChunks.Contains(toUpdate[0].mapPosition) == false)
            {
                activeChunks.Add(toUpdate[0].mapPosition);
            }

            toUpdate.RemoveAt(0);
        }
    }

    void ThreadedUpdate()
    {
        while(true)
        {
            ApplyModifications();

            if (toUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }
    void ThreadedLoad()
    {
        while (true)
        {
            if (toLoad.Count > 0)
            {
                LoadChunk();
            }
        }
    }
    private void OnDisable()
    {
        if (worldData.enableThreading == true)
        {
            chunkUpdateThread.Abort();
            chunkLoadThread.Abort();
        }
    }
    private void LoadChunk()
    {
        lock(loadThreadLock)
        {
            if (toLoad[0] != null)
            {
                toLoad[0].PopulateVoxelMap();
            }

            toLoad.RemoveAt(0);
        }
    }
    private void ApplyModifications()
    {
        lock (updateThreadLock)
        {
            while (modifications.Count > 0)
            {
                Queue<VoxelMod> queue = modifications.Dequeue();

                while (queue.Count > 0)
                {
                    VoxelMod mod = queue.Dequeue();

                    ChunkVector chunkPos = GetChunkVectorFromVector3(mod.position);

                    if (chunkPos == null || chunks.GetLength(0) <= chunkPos.x || chunks.GetLength(1) <= chunkPos.z)
                    {
                        return;
                    }

                    if (chunks[chunkPos.x, chunkPos.z] == null)
                    {
                        return;
                    }


                    chunks[chunkPos.x, chunkPos.z].modifications.Enqueue(mod);
                }
            }
        }
    }

    protected ChunkVector GetChunkVectorFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / chunkWidth);
        int z = Mathf.FloorToInt(pos.z / chunkWidth);

        if (x < 0 || x > worldData.worldSizeInChunks || z < 0 || z > worldData.worldSizeInChunks)
        {
            return null;
        }

        return new ChunkVector(x, z);
    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / chunkWidth);
        int z = Mathf.FloorToInt(pos.z / chunkWidth);

        if(x < 0 || x > worldData.worldSizeInChunks - 1 || z < 0 || z > worldData.worldSizeInChunks - 1)
        {
            return null;
        }

        return chunks[x, z];
    }
    protected void CheckLoadDistance()
    {
        ChunkVector chunkVector = GetChunkVectorFromVector3(player.position);
        lastChunk = currentChunk;

        for (int x = chunkVector.x - loadDistance; x < chunkVector.x + loadDistance; x++)
        {
            for (int z = chunkVector.z - loadDistance; z < chunkVector.z + loadDistance; z++)
            {
                ChunkVector temp = new ChunkVector(x, z);

                if (IsChunkInWorld(temp))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(temp, chunkWidth, chunkHeight);
                    }
                }
            }
        }
    }
    protected void CheckViewDistance()
    {
        ChunkVector chunkVector = GetChunkVectorFromVector3(player.position);
        lastChunk = currentChunk;

        List<ChunkVector> lastActive = new List<ChunkVector>(activeChunks);
        activeChunks.Clear();

        for(int x = chunkVector.x - viewDistance; x < chunkVector.x + viewDistance; x++)
        {
            for (int z = chunkVector.z - viewDistance; z < chunkVector.z + viewDistance; z++)
            {
                ChunkVector temp = new ChunkVector(x, z);

                if (IsChunkInWorld(temp))
                {
                    if (chunks[x, z] != null)
                    {
                        if (chunks[x, z].isActive == false)
                        {
                            chunks[x, z].isActive = true; 
                        }

                        activeChunks.Add(temp);
                    }
                }

                for(int i = 0; i < lastActive.Count; i++)
                {
                    if (lastActive[i].Equals(temp))
                    {
                        lastActive.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < lastActive.Count; i++)
        {
            chunks[lastActive[i].x, lastActive[i].z].isActive = false;
            activeChunks.Remove(lastActive[i]);
        }
    }
    public int GetVoxel(Vector3Int pos)
    {
        int voxelValue = 0;

        int xPos = pos.x;
        int yPos = pos.y; 
        int zPos = pos.z;

        //Fixed Pass

        if (IsVoxelInWorld(pos) == false)
        {
            return 0;
        }

        Vector2Int pos2 = new Vector2Int(xPos, zPos);

        float noise = Perlin.GetHeightMapPerlin(pos2, worldData.scale, offset);

        int terrainHeight = Mathf.FloorToInt(60 + Mathf.Abs(noise * 30));

        if (yPos > terrainHeight)
        {
            if (yPos > worldData.seaLevel)
            {
                return 0;
            }

            return 9;
        }

        BiomeData biome = biomes[Perlin.GetBiomeIndex(pos2, biomes, noise, offset)];

        //Terrain Pass
        if (yPos < terrainHeight - 4)
        {
            voxelValue = 1;
        }
        else if (yPos < terrainHeight)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else
        {
            return 0;
        }

        //Lode Pass
        if (voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Perlin.Get3DPerlin(pos, lode.offset, lode.scale, lode.threshold) == true)
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        //Vegetation pass
        if (yPos == terrainHeight && biome.generateVegetation == true)
        {
            if (Perlin.GetVegetationZoneNoise(pos2, offset, biome.vegetationZoneScale) > biome.vegetationZoneThreshold)
            {
                if (Perlin.GetVegetationDensityNoise(pos2, offset, biome.vegetationDensityScale) > biome.vegetationDensityThreshold)
                {
                    modifications.Enqueue(Structures.GenerateVegetation(biome.vegetationType, pos, biome.minSize, biome.maxSize, offset));
                }
            }
        }

        return voxelValue;
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkVector vector = new ChunkVector(Vector3Int.FloorToInt(pos));

        if(IsVoxelInWorld(Vector3Int.FloorToInt(pos)) == false)
        {
            return false;
        }

        if (chunks[vector.x, vector.z] != null && chunks[vector.x, vector.z].isEditable == true)
        {
            return worldManager.blockData[chunks[vector.x, vector.z].GetVoxelFromVector3(Vector3Int.FloorToInt(pos))].isSolid;
        }

        return worldManager.blockData[GetVoxel(Vector3Int.FloorToInt(pos))].isSolid;
    }
    public bool CheckForVisibleVoxel(Vector3Int pos)
    {
        ChunkVector vector = new ChunkVector(pos);

        if (IsVoxelInWorld(pos) == false)
        {
            return false;
        }

        if (chunks[vector.x, vector.z] != null && chunks[vector.x, vector.z].isEditable == true)
        {
            return worldManager.blockData[chunks[vector.x, vector.z].GetVoxelFromVector3(pos)].hasVisibleNeighbors;
        }

        return worldManager.blockData[GetVoxel(pos)].hasVisibleNeighbors;
    }
    public int GetVoxelFromVector3Int(Vector3Int pos)
    {
        Chunk temp = GetChunkFromVector3(pos);

        if(temp == null)
        {
            return 0;
        }

        return temp.GetVoxelFromVector3(pos);
    }
    protected bool IsChunkInWorld(ChunkVector pos)
    {
        if (pos.x >= 0 && pos.x < chunkCount && pos.z >= 0 && pos.z < chunkCount)
        {
            return true;
        }

        return false;
    }
    protected bool IsVoxelInWorld(Vector3Int pos)
    {
        if (pos.x >= 0 && pos.x < voxelCount && pos.y >= 0 && pos.y < chunkHeight && pos.z >= 0 && pos.z < voxelCount)
        {
            return true;
        }

        return false;
    }
}

public class VoxelMod
{
    public Vector3Int position;
    public int id;

    public VoxelMod(Vector3Int _position, int _id)
    {
        position = _position;
        id = _id;
    }
}





/*private IEnumerator CreateChunks()
  {
  isCreatingChunks = true;

  while(toCreate.Count > 0)
  {
      chunks[toCreate[0].x, toCreate[0].z].Init();
      toCreate.RemoveAt(0);

      yield return null;
  }

  isCreatingChunks = false;
  }*/

/*private void UpdateChunks()
{
    if(currentChunk == null)
    {
        return;
    }

    Vector3 origin = currentChunk.Origin;

    Vector3 chunkCheck = origin + new Vector3(chunkSize, 0, 0);

    if(renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(chunkOffset, 0 ,0)));
    }

    chunkCheck = origin + new Vector3(chunkSize, 0, chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(chunkOffset, 0, chunkOffset)));
    }

    chunkCheck = origin + new Vector3(0, 0, chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(0, 0, chunkOffset)));
    }

    chunkCheck = origin + new Vector3(-chunkSize, 0, chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(-chunkOffset, 0, chunkOffset)));
    }

    chunkCheck = origin + new Vector3(-chunkSize, 0, 0);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(-chunkOffset, 0, 0)));
    }

    chunkCheck = origin + new Vector3(-chunkSize, 0, -chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(-chunkOffset, 0, -chunkOffset)));
    }

    chunkCheck = origin + new Vector3(0, 0, -chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(0, 0, -chunkOffset)));
    }

    chunkCheck = origin + new Vector3(chunkSize, 0, -chunkSize);

    if (renderedChunks.ContainsValue(chunkCheck) == false)
    {
        GenerateChunks(chunkCheck, (origin + new Vector3(chunkOffset, 0, -chunkOffset)));
    }
}*/

/*protected Chunk GenerateChunks(Vector3 origin, Vector3 pos)
{
    if (noiseMap.GetLength(0) < origin.x)
    {
        Debug.Log("Out of range");
        return null;
    }
    else if (noiseMap.GetLength(1) < origin.y)
    {
        Debug.Log("Out of range");
        return null;
    }

    Chunk chunk = Instantiate(chunkPrefab) as Chunk;
    chunk.name = "Chunk";
    chunk.transform.SetParent(transform, true);

    chunk.Init();
    GenerateChunk(chunk, noiseMap);
    renderedChunks.Add(chunk, origin);

    return chunk;
}
protected void GenerateChunk(Chunk chunk, float[,] noiseMap)
{
    for (int x = 0; x < chunk.Width; x++)
    {
        for (int y = 0; y < chunk.Height; y++)
        {
            if ((x + chunk.Origin.x) < 0 || noiseMap.GetLength(0) < (x + chunk.Origin.x))
            {
                Debug.Log("Out of range");

                physicsChunks.Remove(chunk);
                //Destroy(chunk.gameObject);
                return;
            }
            else if ((y + chunk.Origin.y) < 0 || noiseMap.GetLength(1) < (y + chunk.Origin.y))
            {
                Debug.Log("Out of range");
                physicsChunks.Remove(chunk);
                //Destroy(chunk.gameObject);
                return;
            }

            chunk.Data[x, y] = noiseMap[Mathf.FloorToInt(x + chunk.Origin.x), Mathf.FloorToInt(y + chunk.Origin.y)];
        }
    }

   // chunk.GenerateMesh(4, 5);
}*/