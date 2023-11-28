using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Build;
using UnityEditor.TerrainTools;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;

public class ChunkLoader : MonoBehaviour
{
    Transform player;

    [SerializeField]
    public BiomeData defaultBiome;

    private new Camera camera;
    private Vector3 cameraPos;

    WorldManager worldManager;
    //protected float[,] noiseMap;

    protected int chunkCount;
    protected int voxelCount;

    protected int chunkWidth;
    protected int chunkHeight;

    protected int viewDistance;

    Chunk[,] chunks;
    List<ChunkVector> activeChunks = new List<ChunkVector>();

    public ChunkVector currentChunk;
    ChunkVector lastChunk;

    List<ChunkVector> toCreate = new List<ChunkVector>();
    private bool isCreatingChunks;

    private void Start()
    {
        worldManager = WorldManager.instance;
        player = worldManager.player.transform;
        //noiseMap = WorldManager.instance.NoiseMap;

        chunkCount = WorldManager.instance.worldSizeInChunks;
        voxelCount = WorldManager.instance.worldSizeInVoxels;

        chunkWidth = WorldManager.instance.chunkWidth;
        chunkHeight = WorldManager.instance.chunkHeight;

        viewDistance = WorldManager.instance.viewDistanceInChunks;
        chunks = new Chunk[chunkCount, chunkCount];
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

        if(toCreate.Count > 0 && isCreatingChunks == false)
        {
            StartCoroutine(CreateChunks());
        }
    }

    protected void GenerateChunks()
    {
        for(int x = (chunkCount / 2) - viewDistance; x < (chunkCount / 2) + viewDistance; x++)
        {
            for (int z = (chunkCount / 2) - viewDistance; z < (chunkCount / 2) + viewDistance; z++)
            {
                ChunkVector vector = new ChunkVector(x, z);

                chunks[x, z] = new Chunk(this, vector, chunkWidth, chunkHeight, true);
                activeChunks.Add(vector);
            }
        }

        player.position = WorldManager.instance.spawnPosition;
        lastChunk = currentChunk;
    }

    private IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while(toCreate.Count > 0)
        {
            chunks[toCreate[0].x, toCreate[0].z].Init();
            toCreate.RemoveAt(0);

            yield return null;
        }

        isCreatingChunks = false;
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

        return chunks[x, z];
    }
    protected void CheckViewDistance()
    {
        ChunkVector chunkVector = GetChunkVectorFromVector3(player.position);
        lastChunk = currentChunk;

        List<ChunkVector> lastActive = new List<ChunkVector>(activeChunks);

        for(int x = chunkVector.x - viewDistance; x < chunkVector.x + viewDistance; x++)
        {
            for (int z = chunkVector.z - viewDistance; z < chunkVector.z + viewDistance; z++)
            {
                if(IsChunkInWorld(new ChunkVector(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        //CreateChunk(x, z);
                        chunks[x,z] = new Chunk(this, new ChunkVector(x, z), chunkWidth, chunkHeight, false);
                        toCreate.Add(new ChunkVector(x, z));
                    }
                    else
                    {
                        if (chunks[x, z].isActive == false)
                        {
                            chunks[x, z].isActive = true; 
                        }
                    }
                    activeChunks.Add(new ChunkVector(x, z));
                }

                for(int i = 0; i < lastActive.Count; i++)
                {
                    if (lastActive[i].Equals(new ChunkVector(x, z)))
                    {
                        lastActive.RemoveAt(i);
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
        int yPos = Mathf.FloorToInt(pos.y);

        //Generic
        if(IsVoxelInWorld(pos) == false)
        {
            return 0;
        }

        //First Pass
        int terrainHeight = Mathf.FloorToInt(defaultBiome.terrainHeight * worldManager.Get2DPerlin(new Vector2(pos.x, pos.z), defaultBiome.terrainScale, 0)) + defaultBiome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos < terrainHeight - 4)
        {
            voxelValue = 1;
        }
        else if (yPos < terrainHeight - 1)
        {
            voxelValue = 2;
        }
        else if(yPos <= terrainHeight)
        {
            voxelValue =  3;
        }
        else
        {
            return 0;
        }

        //Second Pass
        if(voxelValue != 1)
        {
            return voxelValue;
        }

        foreach(Lode lode in defaultBiome.lodes)
        {
            if(yPos > lode.minHeight && yPos < lode.maxHeight)
            {
                if(worldManager.Get3DPerlin(pos, lode.offset, lode.scale, lode.threshold) == true)
                {
                    voxelValue = lode.blockID;
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

        if (chunks[vector.x, vector.z] != null && chunks[vector.x, vector.z].isMapPopulated == true)
        {
            return worldManager.blockData[chunks[vector.x, vector.z].GetVoxelFromVector3(pos)].isSolid;
        }

        return worldManager.blockData[GetVoxel(pos)].isSolid;
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