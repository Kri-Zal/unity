using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnAheadDistance = 100f;
    public float minObstacleSpacing = 15f;
    public float maxObstacleSpacing = 25f;
    public float laneDistance = 5f;

    [Header("Pool Settings")]
    public int poolSize = 15;

    private List<Obstacle> obstaclePool = new List<Obstacle>();
    private float nextSpawnZ;
    private int[] lanes = { 0, 1, 2 };

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag
            ("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("[ObstacleSpawner] Player not found!");
                enabled = false;
                return;
            }
        }

        InitializePool();
        nextSpawnZ = player.position.z + spawnAheadDistance;
        SpawnInitialObstacles();
    }
    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObstacle();
        }
    }
    void SpawnInitialObstacles()
    {
        float currentZ = player.position.z + 30f;

        for (int i = 0; i < 5; i++)
        {
            SpawnObstacleAt(currentZ);
            currentZ += 
            Random.Range(minObstacleSpacing, maxObstacleSpacing);
        }

        nextSpawnZ = currentZ;
    }

    Obstacle CreateNewObstacle()
    {
        GameObject prefab = 
        obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obj = 
        Instantiate(prefab, Vector3.zero, Quaternion.identity);
        obj.SetActive(false);

        Obstacle obstacle = obj.GetComponent<Obstacle>();
        if (obstacle == null)
        {
            obstacle = obj.AddComponent<Obstacle>();
        }

        obstacle.Initialize(player, this);
        obstaclePool.Add(obstacle);

        return obstacle;
    }
    void Update()
    {
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
        {
            SpawnObstacleAt(nextSpawnZ);
            nextSpawnZ += 
            Random.Range(minObstacleSpacing, maxObstacleSpacing);
        }
    }
    void SpawnObstacleAt(float zPosition)
    {
        Obstacle obstacle = GetPooledObstacle();
        if (obstacle == null) return;

        float[] lanePositions = { -5f, 0f, 5f };
        float xPos = lanePositions[Random.Range(0, lanePositions.Length)];

        Vector3 spawnPos = new Vector3(xPos, 0f, zPosition);
        obstacle.transform.position = spawnPos;
        obstacle.transform.rotation = Quaternion.Euler(0, 180, 0);
        obstacle.gameObject.SetActive(true);
    }

    Obstacle GetPooledObstacle()
    {
        foreach (Obstacle obstacle in obstaclePool)
        {
            if (!obstacle.gameObject.activeInHierarchy)
            {
                return obstacle;
            }
        }

        return CreateNewObstacle();
    }

    public void ReturnObstacle(Obstacle obstacle)
    {
        obstacle.gameObject.SetActive(false);
    }
}
