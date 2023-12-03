using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Item : MonoBehaviour
{
    private PlayerController player;

    private WorldManager worldManager;
    private ChunkLoader chunkLoader;

    private float lifeSpan = 10f;
    private float pickUpRadius = 2f;

    private int blockIndex;
    private GameObject animationObject;

    private Vector3 originalPos;
    private float bobbingStrength = 0.25f;

    private float verticalMomentum;
    private float gravity;
    public void Init(ChunkLoader loader, int index, Sprite sprite)
    {
        worldManager =  WorldManager.instance;
        chunkLoader = loader;
        gravity = worldManager.gravity;
        player = worldManager.player;

        if (worldManager.blockData[index].farmedBlock == 0)
        {
            blockIndex = index;
        }
        else
        {
            blockIndex = worldManager.blockData[index].farmedBlock;
        }

        animationObject = new GameObject();
        animationObject.transform.position = transform.position;
        animationObject.transform.localScale = new Vector3(2, 2, 2);
        animationObject.transform.SetParent(transform, true);

        SpriteRenderer renderer = animationObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        originalPos = animationObject.transform.localPosition;

        worldManager.items.Add(this);
    }

    private void Update()
    {
        Animate();
        transform.Translate(GetVelocity(), Space.World);

        CheckIfPickedUp();
    }

    private void CheckIfPickedUp()
    {
        Vector3 pos = transform.position;
        Vector3 playerPos = player.transform.position;

        Vector3 distance = playerPos - pos;

        if(distance.magnitude < pickUpRadius)
        {
            if(player.toolbar.AddItem(blockIndex, 1) == true)
            {
                WorldManager.instance.items.Remove(this);
                Destroy(gameObject);
            }
        }
    }
    private void Animate()
    {
        Vector3 animPos = new Vector3(animationObject.transform.localPosition.x, originalPos.y + ((float)Math.Sin(Time.time) * bobbingStrength), animationObject.transform.localPosition.z);
        animationObject.transform.localPosition = animPos;
        animationObject.transform.Rotate(0, 0.1f, 0);
    }
    private Vector3 GetVelocity()
    {
        Vector3 velocity = Vector3.zero;

        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if (CheckIfInsideBlock() == true)
        {
            velocity.y += 1;
            return velocity;
        }

        if (velocity.y < 0)
        {
            if(CheckIfGrounded() == true)
            {
                velocity.y = 0;
                return velocity;
            }
        }

        return velocity;
    }
    private bool CheckIfGrounded()
    {
        if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z)) == true)
        {
            return true;
        }

        return false;
    }
    private bool CheckIfInsideBlock()
    {
        if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z)) == true)
        {
            return true;
        }

        return false;
    }
    public void TickLifeSpan(float deltaTime)
    {
        lifeSpan -= deltaTime;

        if(lifeSpan <= 0)
        {
            WorldManager.instance.items.Remove(this);
            Destroy(gameObject);
            return;
        }
    }
}
