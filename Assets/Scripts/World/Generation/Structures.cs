using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structures
{ 
    public static Queue<VoxelMod> GenerateVegetation(int index, Vector3Int position, int minHeight, int maxHeight)
    {
        switch(index)
        {
            case 0:
                return MakeTree(position, minHeight, maxHeight);
            case 1:
                return MakeCacti(position, minHeight, maxHeight);
        }

        return new Queue<VoxelMod>();
    }
    public static Queue<VoxelMod> MakeTree(Vector3Int position, int minHeight, int maxHeight)
    {
        Queue<VoxelMod> queue = new Queue <VoxelMod>();

        int height = (int)(maxHeight * Perlin.GetVegetationNoise(new Vector2Int(position.x, position.y)));

        if(height < minHeight)
        {
            height = minHeight;
        }

        for(int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 4; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3Int(position.x + x, position.y + height + y, position.z + z), 6));
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3Int(position.x, position.y + i, position.z), 5));
        }

        return queue;
    }
    public static Queue<VoxelMod> MakeCacti(Vector3Int position, int minHeight, int maxHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxHeight * Perlin.GetVegetationNoise(new Vector2Int(position.x, position.y)));

        if (height < minHeight)
        {
            height = minHeight;
        }

        for (int i = 1; i <= height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3Int(position.x, position.y + i, position.z), 7));
        }

        return queue;
    }
}
