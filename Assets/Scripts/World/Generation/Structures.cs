using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structures
{ 
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minHeight, int maxHeight)
    {
        Queue<VoxelMod> queue = new Queue <VoxelMod>();

        int height = (int)(maxHeight * WorldManager.instance.Get2DPerlin(new Vector2(position.x, position.y), 0, 0));

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
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 4));
                }
            }
        }

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 1));
        }

        return queue;
    }
}
