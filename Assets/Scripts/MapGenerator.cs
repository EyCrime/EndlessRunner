using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public Camera mainCamera;
    public MapTile tilePrefab;
    public float startMovingSpeed = 12;
    public float boostedSpeedMultiplier = 1f;
    public float movingSpeed;
    public int tilesToPreSpawn = 15; //How many tiles should be pre-spawned
    public int tilesWithoutObstacles = 3; //How many tiles at the beginning should not have obstacles, good for warm-up
    public int level = 1;
    public static MapGenerator instance;
    public GameObject gameStartBtn;
    public TMP_Text scoreText;
    public Animator playerAnimator;

    List<MapTile> spawnedTiles = new List<MapTile>();
    int nextTileToActivate = -1;
    [HideInInspector]
    public bool gameOver = false;
    public bool powerUpActive = false;
    public static bool gameStarted = false;
    private float score = 0;

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
        gameStartBtn.SetActive(!gameStarted);

        var spawnPosition = transform.position;
        var tilesWithNoObstaclesTmp = tilesWithoutObstacles;
        for (var i = 0; i < tilesToPreSpawn; i++)
        {
            spawnPosition -= tilePrefab.startPoint.localPosition;
            var spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
            if(tilesWithNoObstaclesTmp > 0)
            {
                spawnedTile.DeactivateAllObstacles();
                tilesWithNoObstaclesTmp--;
            }
            else
            {
                spawnedTile.ActivateRandomObstacle(level);
                spawnedTile.SpawnCoins();
            }
            
            spawnPosition = spawnedTile.endPoint.position;
            spawnedTile.transform.SetParent(transform);
            spawnedTiles.Add(spawnedTile);
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // Move the object upward in world space x unit/second.
        //Increase speed the higher score we get
        if (!gameOver && gameStarted)
        {
            level = Mathf.CeilToInt(Mathf.Sqrt(score / 500));
            level = level > 5 ? 5 : level;
            
            movingSpeed = startMovingSpeed + score / 500;
            transform.Translate(-spawnedTiles[0].transform.forward * (Time.fixedDeltaTime * (movingSpeed * boostedSpeedMultiplier  + (score/500))), Space.World);
            score += Time.fixedDeltaTime * movingSpeed * boostedSpeedMultiplier;
            scoreText.text = "Score: " + (int)score;
        }

        if (mainCamera.WorldToViewportPoint(spawnedTiles[0].endPoint.position).z < 0)
        {
            //Move the tile to the front if it's behind the Camera
            var tileTmp = spawnedTiles[0];
            spawnedTiles.RemoveAt(0);
            tileTmp.transform.position = spawnedTiles[spawnedTiles.Count - 1].endPoint.position - tileTmp.startPoint.localPosition;
            tileTmp.ActivateRandomObstacle(level);
            tileTmp.SpawnCoins();
            spawnedTiles.Add(tileTmp);
        }

        
    }

    private void Update()
    {
        if (gameOver)
        {
            gameStartBtn.SetActive(true);
        }
    }

    public void GameStart()
    {
        if (gameOver)
        {
            //Restart current scene
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);            
        }
        else
        {
            //Start the game
            gameStarted = true;
            gameStartBtn.SetActive(false);
            playerAnimator.SetBool("running", true);
        }
    }
}