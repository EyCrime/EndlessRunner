using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapTile : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public List<GameObject> obstacles;
    public List<GameObject> powerUps;
    public GameObject coinPrefab;
    public float[] xPositions;

    private List<int> _gapIndices;
    private int _powerUpIndex;

    public void ActivateRandomObstacle(int level)
    {
        DeactivateAllObstacles();

        var randomObstacleCount = Mathf.Clamp(Random.Range(level - 1, level + 2), 0, 5);
        
        for (var i = 0; i < randomObstacleCount; i++)
        {
            var randomNumber = Random.Range(0, _gapIndices.Count);
            var obstacleIndex = _gapIndices[randomNumber];
            obstacles[obstacleIndex].SetActive(true);
            _gapIndices.Remove(obstacleIndex);
        }
        
        // only one powerUp can be Active at any time
        if (!MapGenerator.instance.powerUpActive)
            ActivateRandomPowerUp();
    }
    
    private void ActivateRandomPowerUp()
    {
        DeactivatePowerUp();
        
        // 10% chance to spawn power up
        if (Random.Range(0, 10) != 0) return;
        
        var randomNumber = Random.Range(0, _gapIndices.Count);
        _powerUpIndex = _gapIndices[randomNumber];
        
        var powerUp = powerUps[_powerUpIndex];
        powerUp.SetActive(true);
        powerUp.GetComponent<PowerUp>().SetRandomType();

        MapGenerator.instance.powerUpActive = true;
        
        _gapIndices.Remove(_powerUpIndex);
    }

    public void SpawnCoins()
    {
        DestroyAllCoins();
        
        // 50% chance to spawn coins
        if (Random.Range(0, 2) != 0) return;
        
        var randomLaneCount = Random.Range(1, 4);

        var laneIndices = new List<int>() {0, 1, 2};
        
        for (var i = 0; i < randomLaneCount; i++)
        {
            var randomLaneIndex = laneIndices[Random.Range(0, laneIndices.Count)];

            var startPos = new Vector3(xPositions[randomLaneIndex], coinPrefab.transform.position.y, transform.position.z);
            laneIndices.Remove(randomLaneIndex);

            var middlePos = startPos;
            
            if (obstacles[randomLaneIndex].activeInHierarchy) // bot obstacle of this lane is active
            {
                if (obstacles[randomLaneIndex + 3].activeInHierarchy) // top obstacle of this lane is also active
                    return; // don't spawn coins here, the lane is blocked
                else
                    middlePos.y = 3.5f;
            }

            // If there is no power up spawn coin in mid
            if (_powerUpIndex != randomLaneIndex)
                Instantiate(coinPrefab, middlePos, coinPrefab.transform.rotation, transform);
            
            var randomCoinCount = Random.Range(2, 10); // 5 - 19 coins
            
            for (var j = 1; j <= randomCoinCount; j++)
            {
                var pos1 = startPos;
                var pos2 = startPos;

                if (j == 1)
                {
                    pos1 = middlePos;
                    pos2 = middlePos;
                }
                
                pos1.z += j;
                pos2.z -= j;
                
                Instantiate(coinPrefab, pos1, coinPrefab.transform.rotation, transform);
                Instantiate(coinPrefab, pos2, coinPrefab.transform.rotation, transform);
            }
        }
    }

    public void DeactivateAllObstacles()
    {
        _gapIndices = new List<int>() {0, 1, 2, 3, 4, 5};
        _powerUpIndex = -1;
        
        for (var i = 0; i < obstacles.Count; i++)
        {
            obstacles[i].SetActive(false);
        }
    }
    
    public void DeactivatePowerUp()
    {
        for (var i = 0; i < powerUps.Count; i++)
        {
            if (powerUps[i].activeInHierarchy)
            {
                powerUps[i].SetActive(false);
                MapGenerator.instance.powerUpActive = false;
            }
        }
    }

    public void DestroyAllCoins()
    {
        var coins = GetComponentsInChildren<Coin>();
        
        for (var i = 0; i < coins.Length; i++)
        {
            Destroy(coins[i].gameObject);
        }
    }
}
