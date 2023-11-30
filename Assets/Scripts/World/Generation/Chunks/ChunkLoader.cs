using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class ChunkLoader : MonoBehaviour
{
    Transform player;

    [SerializeField]
    public BiomeData defaultBiome;

    private new Camera camera;
    private Vector3 cameraPos;

    WorldManager worldManager;
    protected float[,] noiseMap;

    protected int chunkCount;
    protected int voxelCount;

    protected int chunkWidth;
    protected int chunkHeight;

    protected int viewDistance;

    Chunk[,] chunks;
    List<ChunkVector> activeChunks = new List<ChunkVector>();

    public ChunkVector currentChunk;
    ChunkVector lastChunk;

    public Queue<Chunk> toDraw = new Queue<Chunk>();

    List<ChunkVector> toCreate = new List<ChunkVector>();
    public List<Chunk> toUpdate = new List<Chunk>();

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    private bool applyingMods = false;

    Thread chunkUpdateThread;
    public object updateThreadLock = new object();

    private int readyIndex = 0;
    public bool isReady = false;

    private void Start()
    {
        worldManager = WorldManager.instance;
        player = worldManager.player.transform;

        chunkCount = WorldManager.instance.worldSizeInChunks;
        voxelCount = WorldManager.instance.worldSizeInVoxels;

        chunkWidth = WorldManager.instance.chunkWidth;
        chunkHeight = WorldManager.instance.chunkHeight;

        viewDistance = WorldManager.instance.viewDistanceInChunks;

        chunks = new Chunk[chunkCount, chunkCount];
        noiseMap = worldManager.noiseMap;

        if(worldManager.enableThreading == true)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }


        GenerateChunks();
    }

    private void Update()
    {
        currentChunk = GetChunkVectorFromVector3(player.position);

        if (currentChunk.Equals(lastChunk) == false)
        {
            CheckViewDistance();
            lastChunk = currentChunk;
        }

        if(toCreate.Count > 0)
        {
            CreateChunk();

            if (isReady == false && toCreate.Count == 0)
            {
                isReady = true;
            }
        }

        if (toDraw.Count > 0)
        {
            if (toDraw.Peek().isEditable == true)
            {
                toDraw.Dequeue().CreateMesh();
            }
        }

        if (worldManager.enableThreading == false)
        {
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

    protected void GenerateChunks()
    {
        for(int x = (chunkCount / 2) - viewDistance; x < (chunkCount / 2) + viewDistance; x++)
        {
            for (int z = (chunkCount / 2) - viewDistance; z < (chunkCount / 2) + viewDistance; z++)
            {
                ChunkVector vector = new ChunkVector(x, z);

                chunks[x, z] = new Chunk(this, vector, chunkWidth, chunkHeight);
                toCreate.Add(vector);
            }
        }

        player.position = WorldManager.instance.spawnPosition;
        lastChunk = currentChunk;

        CheckViewDistance();
    }

    private void CreateChunk()
    {
        ChunkVector pos = toCreate[0];
        toCreate.RemoveAt(0);

        chunks[pos.x, pos.z].Init();
    }
    private void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock(updateThreadLock)
        {
            while (updated == false && index < toUpdate.Count - 1)
            {
                if (toUpdate[index].isEditable == true)
                {
                    toUpdate[index].UpdateChunk();

                    if (activeChunks.Contains(toUpdate[index].mapPosition) == false)
                    {
                        activeChunks.Add(toUpdate[index].mapPosition);
                    }

                    toUpdate.RemoveAt(index);

                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ThreadedUpdate()
    {
        while(true)
        {
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
    private void OnDisable()
    {
        if (worldManager.enableThreading == true)
        {
            chunkUpdateThread.Abort();
        }
    }
    private void ApplyModifications()
    {
        applyingMods = true;

        while(modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while(queue.Count > 0)
            {
                VoxelMod mod = queue.Dequeue();

                ChunkVector chunkPos = GetChunkVectorFromVector3(mod.position);

                if (chunks[chunkPos.x, chunkPos.z] == null)
                {
                    chunks[chunkPos.x, chunkPos.z] = new Chunk(this, chunkPos, chunkWidth, chunkHeight);
                    toCreate.Add(chunkPos);
                }

                chunks[chunkPos.x, chunkPos.z].modifications.Enqueue(mod);
            }
        }

        applyingMods = false;
    }

    protected ChunkVector GetChunkVectorFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / chunkWidth);
        int z = Mathf.FloorToInt(pos.z / chunkWidth);

        return new ChunkVector(x, z);
    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / chunkWidth);
        int z = Mathf.FloorToInt(pos.z / chunkWidth);

        if(x < 0 || x > worldManager.worldSizeInChunks || z < 0 || x > worldManager.worldSizeInChunks)
        {
            return null;
        }

        return chunks[x, z];
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
                    if (chunks[x, z] == null)
                    {
                        chunks[x,z] = new Chunk(this, temp, chunkWidth, chunkHeight);
                        toCreate.Add(temp);
                    }
                    else
                    {
                        if (chunks[x, z].isActive == false)
                        {
                            chunks[x, z].isActive = true; 
                        }
                    }

                    activeChunks.Add(temp);
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
    public byte GetVoxel(Vector3 pos)
    {
        int xPos = Mathf.FloorToInt(pos.x);
        int yPos = Mathf.FloorToInt(pos.y);

        //Generic
        if(IsVoxelInWorld(pos) == false)
        {
            return 0;
        }

        //First Pass
        int terrainHeight = Mathf.FloorToInt(defaultBiome.terrainHeight * worldManager.Get2DPerlin(new Vector2(pos.x, pos.z), defaultBiome.terrainScale, 0)) + defaultBiome.solidGroundHeight;
        byte voxelValue = 0;

        if (worldManager.isSpawned == false && pos.x == worldManager.spawnPosition.x && pos.z == worldManager.spawnPosition.z)
        {
            worldManager.spawnPosition = new Vector3(pos.x, pos.y + 4, pos.z);
        }


        if (yPos < terrainHeight - 4)
        {
            voxelValue = 1;
        }
        else if (yPos < terrainHeight)
        {
            voxelValue = 2;
        }
        else if(yPos == terrainHeight)
        {
            voxelValue =  3;
        }
        else
        {
            return 0;
        }

        //Second Pass
        if(voxelValue == 1)
        {
            foreach (Lode lode in defaultBiome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (worldManager.Get3DPerlin(pos, lode.offset, lode.scale, lode.threshold) == true)
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        //Third pass

        if(yPos == terrainHeight)
        {
            if (worldManager.Get2DPerlin(new Vector2(pos.x, pos.z), defaultBiome.treeZoneScale, 0) > defaultBiome.treeZoneThreshold)
            {
                if(worldManager.Get2DPerlin(new Vector2(pos.x, pos.z), defaultBiome.treePlacementScale, 0) > defaultBiome.treePlacementThreshold)
                {
                   modifications.Enqueue(Structures.MakeTree(pos, defaultBiome.minTreeSize, defaultBiome.maxTreeSize));
                }
            }
        }

        return voxelValue;
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkVector vector = new ChunkVector(pos);

        if(IsVoxelInWorld(pos) == false)
        {
            return false;
        }

        if (chunks[vector.x, vector.z] != null && chunks[vector.x, vector.z].isEditable == true)
        {
            return worldManager.blockData[chunks[vector.x, vector.z].GetVoxelFromVector3(pos)].isSolid;
        }

        return worldManager.blockData[GetVoxel(pos)].isSolid;
    }
    public int GetVoxelFromVector3(Vector3 pos)
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
        if (pos.x > 0 && pos.x < chunkCount - 1 && pos.z > 0 && pos.z < chunkCount - 1)
        {
            return true;
        }

        return false;
    }
    protected bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < voxelCount && pos.y >= 0 && pos.y < chunkHeight && pos.z >= 0 && pos.z < voxelCount)
        {
            return true;
        }

        return false;
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
}

public class VoxelMod
{
    public Vector3 position;
    public int id;

    public VoxelMod(Vector3 _position, int _id)
    {
        position = _position;
        id = _id;
    }
}