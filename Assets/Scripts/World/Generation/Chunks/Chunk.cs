using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    public ChunkVector mapPosition;

    GameObject chunkObject;
    public Vector3 position;

    MeshFilter meshFilter;
    Renderer meshRenderer;

    WorldManager worldManager;
    ChunkLoader chunkLoader;

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    protected int width;
    public int Width {  get { return width; } }

    protected int height;
    public int Height { get { return height; } }

    protected float[,] data;
    public float[,] Data 
    { 
        get { return data; } 
        set { data = value; }
    }

    int vIndex = 0;

    Material[] materials = new Material[2];
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    //private List<int> transparentTris = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();

    public int[,,] voxelMap;
    private bool _isActive;
    private bool isMapPopulated;

    public Chunk(ChunkVector mapPos ,int _width, int _height)
    {
        mapPosition = mapPos;
        worldManager = WorldManager.instance;
        chunkLoader = ChunkLoader.instance;

        width = _width;
        height = _height;
    }

    public void Init()
    {
        chunkObject = new GameObject();
        chunkObject.transform.SetParent(worldManager.transform);
        chunkObject.transform.position = new Vector3(mapPosition.x * width, 0f, mapPosition.z * width);
        chunkObject.name = "Chunk: " + chunkObject.transform.position.x + "_" + chunkObject.transform.position.z;

        position = chunkObject.transform.position;

        meshFilter = chunkObject.AddComponent<MeshFilter>();

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = worldManager.blockMaterial;
        //materials[1] = worldManager.transparentBlockMaterial;

        meshRenderer.material = materials[0];

        voxelMap = new int[width, height, width];

        PopulateVoxelMap();
    }

    public void UpdateChunk()
    {
        while(modifications.Count > 0)
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
                        AddVoxel(new Vector3(x, y, z));
                    }
                }
            }
        }

        lock (chunkLoader.toDraw)
        {
            chunkLoader.toDraw.Enqueue(this);
        }
    }

    protected void PopulateVoxelMap()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    voxelMap[x, y, z] = chunkLoader.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }

        isMapPopulated = true;

        lock(chunkLoader.updateThreadLock)
        {
            chunkLoader.toUpdate.Add(this);
        }
    }

    protected bool CheckVoxelIsTransparent(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (isVoxelInChunk(x, y, z) == false)
        {
            return chunkLoader.CheckForTransparentVoxel(pos + position);
        }

        return WorldManager.instance.blockData[voxelMap[x, y, z]].isTransparent;
    }

    protected bool CheckVoxelIsSolid(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (isVoxelInChunk(x, y, z) == false)
        {
            return chunkLoader.CheckForVoxel(pos + position);
        }

        return WorldManager.instance.blockData[voxelMap[x, y, z]].isSolid;
    }
    public int GetVoxelFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x -= Mathf.FloorToInt(position.x);
        z -= Mathf.FloorToInt(position.z);

        if(x < 0 || x > worldManager.worldSizeInVoxels || z < 0 || z > worldManager.worldSizeInVoxels || y > worldManager.chunkHeight)
        {
            return 0;
        }

        return voxelMap[x, y, z];
    }
    protected void AddVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        int blockID = voxelMap[x, y, z];
        //bool isTransparent = worldManager.blockData[blockID].isTransparent;

        for (int i = 0; i < 6; i++)
        {
            if (CheckVoxelIsSolid(pos + VoxelData.faceChecks[i]) == false)
            {
                for (int u = 0; u < 4; u++)
                {
                    verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, u]]);
                    normals.Add(VoxelData.faceChecks[i]);
                }

                AddTexture(WorldManager.instance.blockData[blockID].GetTextureID(i));

                //if(isTransparent == false)
                //{
                    tris.Add(vIndex);
                    tris.Add(vIndex + 1);
                    tris.Add(vIndex + 2);
                    tris.Add(vIndex + 2);
                    tris.Add(vIndex + 1);
                    tris.Add(vIndex + 3);
                /*}
                else
                {
                    transparentTris.Add(vIndex);
                    transparentTris.Add(vIndex + 1);
                    transparentTris.Add(vIndex + 2);
                    transparentTris.Add(vIndex + 2);
                    transparentTris.Add(vIndex + 1);
                    transparentTris.Add(vIndex + 3);
                }*/

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
        if(x < 0 || x > worldManager.chunkWidth - 1 || y < 0 || y > worldManager.chunkHeight - 1 || z < 0 || z > worldManager.chunkWidth - 1)
        {
            return false;
        }

        return true;
    }

    public bool EditVoxel(Vector3 pos, int data)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if(y > height - 1)
        {
            return false;
        }

        Vector3 itemPos = new Vector3(x, y, z);
        int oldData = 0;

        x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        if (voxelMap[x, y, z] != 0)
        {
            oldData = voxelMap[x, y, z];
        }

        voxelMap[x, y, z] = data;

        if(oldData != 0 && voxelMap[x, y, z] == 0)
        {
            SpawnItem(oldData, itemPos);
        }

        lock(chunkLoader.updateThreadLock)
        {
            chunkLoader.toUpdate.Insert(0, this);
            UpdateSurroundingVoxels(x, y, z);
        }

        return true;
    }

    private void SpawnItem(int blockID, Vector3 pos)
    {
        GameObject newItem = new GameObject();
        newItem.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f);
        newItem.name = worldManager.blockData[blockID].blockName + " Item";

        Item item = newItem.AddComponent<Item>();
        item.Init(chunkLoader ,blockID, worldManager.blockData[blockID].displayImage);
    }
    protected void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 currentVoxel = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 temp = currentVoxel + VoxelData.faceChecks[i];

            if(isVoxelInChunk((int)temp.x, (int)temp.y, (int)temp.z) == false)
            {
                chunkLoader.toUpdate.Insert(0, (chunkLoader.GetChunkFromVector3(temp + position)));
            }
        }
    }
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts.ToArray();

        //mesh.subMeshCount = 2;

        //mesh.SetTriangles(tris.ToArray(), 0);
        //mesh.SetTriangles(transparentTris.ToArray(), 1);

        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }

    public void ClearMeshData()
    {
        vIndex = 0;
        verts.Clear();
        tris.Clear();
        //transparentTris.Clear();
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
    public ChunkVector(Vector3 pos)
    {
        int _x = Mathf.FloorToInt(pos.x);
        int _z = Mathf.FloorToInt(pos.z);

        x = _x / WorldManager.instance.chunkWidth;
        z = _z / WorldManager.instance.chunkWidth;
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