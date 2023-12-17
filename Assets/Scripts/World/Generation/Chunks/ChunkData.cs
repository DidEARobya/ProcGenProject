using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    ChunkLoader chunkLoader;
    public ChunkVector mapPosition;
    public Vector3Int position;

    public int width;
    public int height;

    public int[,,] voxelMap;
    public bool isLoaded;

    public ChunkData(ChunkVector mapPos, int _width, int _height)
    {
        chunkLoader = ChunkLoader.instance;

        mapPosition = mapPos;

        width = _width;
        height = _height;

        voxelMap = new int[width, height, width];
    }

    public int[,,] PopulateVoxelMap()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    if(voxelMap == null)
                    {
                        return null;
                    }

                    voxelMap[x, y, z] = chunkLoader.GetVoxel(new Vector3Int(x, y, z) + position);
                }
            }
        }

        isLoaded = true;
        return voxelMap;
    }
}
