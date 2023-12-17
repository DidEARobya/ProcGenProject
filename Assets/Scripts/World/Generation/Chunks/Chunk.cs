using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    public ChunkVector mapPosition;
    public ChunkData data;

    GameObject chunkObject;
    public Vector3Int position;

    MeshFilter meshFilter;
    Renderer meshRenderer;

    WorldManager worldManager;
    WorldData worldData;

    ChunkLoader chunkLoader;

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    protected int width;
    protected int height;

    int vIndex = 0;

    Material[] materials = new Material[2];
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private List<int> transparentTris = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();

    public int[,,] voxelMap;
    private bool _isActive;
    private bool isMapPopulated;

    public Chunk(ChunkData _data)
    {
        data = _data;
        width = data.width;
        height = data.height;
        mapPosition = data.mapPosition;

        worldManager = WorldManager.instance;
        worldData = worldManager.worldData;
        chunkLoader = ChunkLoader.instance;

        chunkObject = new GameObject();
        chunkObject.transform.SetParent(worldManager.transform);
        chunkObject.transform.position = new Vector3(mapPosition.x * width, 0f, mapPosition.z * width);
        chunkObject.name = "Chunk: " + chunkObject.transform.position.x + "_" + chunkObject.transform.position.z;

        chunkObject.SetActive(false);
        _isActive = false;

        position = Vector3Int.FloorToInt(chunkObject.transform.position);
        data.position = position;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = worldManager.blockMaterial;
        materials[1] = worldManager.transparentBlockMaterial;

        meshRenderer.materials = materials;

        chunkLoader.toLoad.Add(this);
    }

    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod mod = modifications.Dequeue();
            Vector3 pos = (mod.position -= position);

            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = mod.id;
        }

        ClearMeshData();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    if (worldManager.blockData[voxelMap[x, y, z]].isSolid == true)
                    {
                        AddVoxel(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        lock (chunkLoader.toDraw)
        {
            chunkLoader.toDraw.Enqueue(this);
        }
    }

    public bool LoadChunk()
    {
        if(data.isLoaded == false)
        {
            voxelMap = data.PopulateVoxelMap();
        }
        else
        {
            voxelMap = data.voxelMap;
        }

        if (voxelMap == null)
        {
            return false;
        }

        isMapPopulated = true;
        chunkLoader.toUpdate.Add(this);

        return true;
    }

    protected bool CheckIfVoxelHasVisibleNeighbors(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        if (isVoxelInChunk(x, y, z) == false)
        {
            return chunkLoader.CheckForVisibleVoxel(pos + position);
        }

        return WorldManager.instance.blockData[voxelMap[x, y, z]].hasVisibleNeighbors;
    }

    protected bool CheckVoxelIsSolid(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        if (isVoxelInChunk(x, y, z) == false)
        {
            return chunkLoader.CheckForVoxel(pos + position);
        }

        return WorldManager.instance.blockData[voxelMap[x, y, z]].isSolid;
    }
    public int GetVoxelFromVector3(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        x -= position.x;
        z -= position.z;

        if(x < 0 || x > worldData.worldSizeInVoxels || z < 0 || z > worldData.worldSizeInVoxels || y > worldData.chunkHeight)
        {
            return 0;
        }

        return voxelMap[x, y, z];
    }
    protected void AddVoxel(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        int blockID = voxelMap[x, y, z];
        bool hasVisibleNeighbors = worldManager.blockData[blockID].hasVisibleNeighbors;

        for (int i = 0; i < 6; i++)
        {
            if (CheckVoxelIsSolid(pos + VoxelData.faceChecks[i]) == false && CheckIfVoxelHasVisibleNeighbors(pos + VoxelData.faceChecks[i]) == true)
            {
                for (int u = 0; u < 4; u++)
                {
                    verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, u]]);
                    normals.Add(VoxelData.faceChecks[i]);
                }

                AddTexture(WorldManager.instance.blockData[blockID].GetTextureID(i));

                if (hasVisibleNeighbors == false)
                {
                    tris.Add(vIndex);
                    tris.Add(vIndex + 1);
                    tris.Add(vIndex + 2);
                    tris.Add(vIndex + 2);
                    tris.Add(vIndex + 1);
                    tris.Add(vIndex + 3);
                }
                else
                {
                    transparentTris.Add(vIndex);
                    transparentTris.Add(vIndex + 1);
                    transparentTris.Add(vIndex + 2);
                    transparentTris.Add(vIndex + 2);
                    transparentTris.Add(vIndex + 1);
                    transparentTris.Add(vIndex + 3);
                }

                vIndex += 4;
            }
        }
    }

    void AddTexture(int texID)
    {
        float y = texID / VoxelData.TextureAtlasSize;
        float x = texID - (y * VoxelData.TextureAtlasSize);

        x *= VoxelData.NormalisedBlockTextureSize;
        y *= VoxelData.NormalisedBlockTextureSize;

        y = 1f - y - VoxelData.NormalisedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalisedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y + VoxelData.NormalisedBlockTextureSize));
    }

    bool isVoxelInChunk(int x, int y, int z)
    {
        if(x < 0 || x > worldData.chunkWidth - 1 || y < 0 || y > worldData.chunkHeight - 1 || z < 0 || z > worldData.chunkWidth - 1)
        {
            return false;
        }

        return true;
    }

    public bool EditVoxel(Vector3Int pos, int data)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        if(y > height - 1)
        {
            return false;
        }

        Vector3Int itemPos = new Vector3Int(x, y, z);
        int oldData = 0;

        x -= position.x;
        z -= position.z;

        if (voxelMap[x, y, z] != 0)
        {
            oldData = voxelMap[x, y, z];
        }

        voxelMap[x, y, z] = data;

        if(oldData != 0 && voxelMap[x, y, z] == 0)
        {
            SpawnItem(oldData, itemPos);
        }

        lock (chunkLoader.updateThreadLock)
        {
            chunkLoader.toUpdate.Insert(0, this);
            UpdateSurroundingVoxels(x, y, z);
        }

        return true;
    }

    private void SpawnItem(int blockID, Vector3Int pos)
    {
        GameObject newItem = new GameObject();
        newItem.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f);
        newItem.name = worldManager.blockData[blockID].blockName + " Item";

        Item item = newItem.AddComponent<Item>();
        item.Init(chunkLoader ,blockID, worldManager.blockData[blockID].displayImage);
    }
    protected void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3Int currentVoxel = new Vector3Int(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3Int temp = currentVoxel + VoxelData.faceChecks[i];

            if(isVoxelInChunk(temp.x, temp.y, temp.z) == false)
            {
                chunkLoader.toUpdate.Add(chunkLoader.GetChunkFromVector3(temp + position));
            }
        }
    }

    public ChunkData SaveChunk()
    {
        Object.Destroy(chunkObject.gameObject);

        if(voxelMap != null)
        {
            data.voxelMap = voxelMap;
        }

        return data;
    }
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(tris.ToArray(), 0);
        mesh.SetTriangles(transparentTris.ToArray(), 1);

        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        if(meshFilter == null)
        {
            return;
        }

        meshFilter.mesh = mesh;
    }

    public void ClearMeshData()
    {
        vIndex = 0;
        verts.Clear();
        tris.Clear();
        transparentTris.Clear();
        uvs.Clear();
        normals.Clear();
    }
    public bool isActive
    {
        get { return _isActive; }
        set 
        { 
            _isActive = value;

            if(chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }
    public bool isEditable
    {
        get 
        { 
            if(isMapPopulated == false)
            {
                return false;
            }

            return true;
        }
    }
}

public class ChunkVector
{
    public int x;
    public int z;

    public ChunkVector()
    {
        x = 0;
        z = 0;
    }
    public ChunkVector(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    public ChunkVector(Vector3Int pos)
    {
        int _x = pos.x;
        int _z = pos.z;

        x = _x / WorldManager.instance.worldData.chunkWidth;
        z = _z / WorldManager.instance.worldData.chunkWidth;
    }
    public bool Equals(ChunkVector other)
    {
        if(other == null)
        {
            return false;
        }

        if(other.x == x && other.z == z)
        {
            return true;
        }

        return false;
    }
}



/*public void PopulateVoxelMap()
{
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                voxelMap[x, y, z] = chunkLoader.GetVoxel(new Vector3Int(x, y, z) + position);
            }
        }
    }

    isMapPopulated = true;

    chunkLoader.toUpdate.Add(this);
}*/