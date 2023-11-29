using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    protected WorldManager worldManager;
    protected ChunkLoader chunkLoader;
    protected PlayerController playerController;

    [SerializeField]
    public TextMeshProUGUI text;
    private GameObject textObject;

    float frameRate;
    float timer;

    int halfWorldSizeInChunks;
    int halfWorldSizeInVoxels;

    private void Start()
    {
        worldManager = WorldManager.instance;
        chunkLoader = worldManager.gameObject.GetComponent<ChunkLoader>();
        playerController = worldManager.player.GetComponent<PlayerController>();

        halfWorldSizeInChunks = worldManager.worldSizeInChunks / 2;
        halfWorldSizeInVoxels = worldManager.worldSizeInVoxels / 2;

        textObject = text.gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            textObject.SetActive(!textObject.activeSelf);
        }

        timer += Time.deltaTime;

        if (timer > 1)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }

        if (textObject.activeSelf == false)
        {
            return;
        }

        string debugText = "Debugging...";
        debugText += "\n\n";
        debugText += frameRate.ToString() + " fps";
        debugText += "\n\n";
        debugText += "XYZ: " + Mathf.FloorToInt((worldManager.player.transform.position.x) - halfWorldSizeInVoxels) + "," + Mathf.FloorToInt(worldManager.player.transform.position.y) + "," + Mathf.FloorToInt((worldManager.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n\n";

        if(chunkLoader.currentChunk != null)
        {
            debugText += "Chunk - XZ: ";
            debugText += (chunkLoader.currentChunk.x - halfWorldSizeInChunks).ToString() + "," + (chunkLoader.currentChunk.z - halfWorldSizeInChunks).ToString();
        }

        text.text = debugText;
    }
}
