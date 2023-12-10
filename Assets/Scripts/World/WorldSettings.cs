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

    public float scale = 0.1f;
    public float lacunarity = 2.0f;
    public float persistence = 0.5f;
    public int octaves = 4;
    public int seed = 32;

    public int chunkWidth = 16;
    public int chunkHeight = 64;

    public int loadDistance = 5;
    public int viewDistanceInChunks = 1;

    [SerializeField]
    public Slider seedSlider;
    [SerializeField]
    public TextMeshProUGUI seedText;

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

    [SerializeField]
    public Slider loadDistanceSlider;
    [SerializeField]
    public TextMeshProUGUI loadDistanceText;

    [SerializeField]
    public Slider perlinScaleSlider;
    [SerializeField]
    public TextMeshProUGUI perlinScaleText;

    [SerializeField]
    public Slider perlinLacunaritySlider;
    [SerializeField]
    public TextMeshProUGUI perlinLacunarityText;

    [SerializeField]
    public Slider perlinPersistenceSlider;
    [SerializeField]
    public TextMeshProUGUI perlinPersistenceText;

    [SerializeField]
    public Slider perlinOctavesSlider;
    [SerializeField]
    public TextMeshProUGUI perlinOctavesText;
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
    public void SetLoadDistance()
    {
        loadDistance = Mathf.FloorToInt(loadDistanceSlider.value);
        loadDistanceText.text = "Load Distance: " + loadDistance.ToString();
    }
    public void SetScale()
    {
        scale = (float)System.Math.Round(perlinScaleSlider.value, 2);
        perlinScaleText.text = "Perlin Scale: " + scale.ToString();
    }
    public void SetLacunarity()
    {
        lacunarity = (float)System.Math.Round(perlinLacunaritySlider.value, 2);
        perlinLacunarityText.text = "Perlin Lacunarity: " + lacunarity.ToString();
    }
    public void SetPersistence()
    {
        persistence = (float)System.Math.Round(perlinPersistenceSlider.value, 2);
        perlinPersistenceText.text = "Perlin Persistence: " + persistence.ToString();
    }
    public void SetOctaves()
    {
        octaves = Mathf.FloorToInt(perlinOctavesSlider.value);
        perlinOctavesText.text = "Perlin Octaves: " + octaves.ToString();
    }
    public void GenerateWorld()
    {
        SceneManager.LoadScene(1);
    }
}
