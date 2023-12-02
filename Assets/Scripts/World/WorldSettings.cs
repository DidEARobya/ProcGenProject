using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSettings : MonoBehaviour
{
    public static WorldSettings instance;

    public bool enableThreading;

    public int seed = 32;
    public int seedOffset = 0;

    public int worldSizeInChunks = 20;
    public int chunkWidth = 16;
    public int chunkHeight = 64;

    public int viewDistanceInChunks = 5;

    [SerializeField]
    public Slider seedSlider;
    [SerializeField]
    public TextMeshProUGUI seedText;

    [SerializeField]
    public Slider offsetSlider;
    [SerializeField]
    public TextMeshProUGUI offsetText;

    [SerializeField]
    public Slider worldSizeSlider;
    [SerializeField]
    public TextMeshProUGUI worldSizeText;

    [SerializeField]
    public Slider chunkWidthSlider;
    [SerializeField]
    public TextMeshProUGUI chunkWidthText;

    [SerializeField]
    public Slider chunkHeightSlider;
    [SerializeField]
    public TextMeshProUGUI chunkHeightText;

    [SerializeField]
    public Slider viewDistanceSlider;
    [SerializeField]
    public TextMeshProUGUI viewDistanceText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ToggleThreading()
    {
        enableThreading = !enableThreading;
    }

    public void SetSeed()
    {
        seed = Mathf.FloorToInt(seedSlider.value);
        seedText.text = "Seed: " + seed.ToString();
    }
    public void SetOffset()
    {
        seedOffset = Mathf.FloorToInt(offsetSlider.value);
        offsetText.text = "Offset: " + seedOffset.ToString();
    }
    public void SetWorldSize()
    {
        worldSizeInChunks = Mathf.FloorToInt(worldSizeSlider.value);
        worldSizeText.text = "World Size: " + worldSizeInChunks.ToString();
    }
    public void SetChunkWidth()
    {
        chunkWidth = Mathf.FloorToInt(chunkWidthSlider.value);
        chunkWidthText.text = "Chunk Width: " + chunkWidth.ToString();
    }
    public void SetChunkHeight()
    {
        chunkHeight = Mathf.FloorToInt(chunkHeightSlider.value);
        chunkHeightText.text = "Chunk Height: " + chunkHeight.ToString();
    }
    public void SetViewDistance()
    {
        viewDistanceInChunks = Mathf.FloorToInt(viewDistanceSlider.value);
        viewDistanceText.text = "View Distance: " + viewDistanceInChunks.ToString();
    }
    public void GenerateWorld()
    {
        SceneManager.LoadScene(1);
    }
}
