using UnityEngine;
using System.Collections.Generic;

public class InfiniteTileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;   // your empty prefab
    public int tileCount = 5;       // number of tiles
    public float tileLength = 50f;  // manually set this to match your prefab size
    public Transform playerTransform;

    private List<GameObject> tiles = new List<GameObject>();
    private float nextSpawnZ;

    void Start()
    {
        // Spawn initial tiles
        for (int i = 0; i < tileCount; i++)
        {
            GameObject t = Instantiate(tilePrefab, Vector3.forward * nextSpawnZ, Quaternion.identity, transform);
            tiles.Add(t);
            nextSpawnZ += tileLength;
        }
    }

    void Update()
    {
        // Move tile from front to end when player passes it
        while (playerTransform.position.z - tiles[0].transform.position.z > tileLength)
        {
            GameObject firstTile = tiles[0];
            firstTile.transform.position = Vector3.forward * nextSpawnZ;
            nextSpawnZ += tileLength;

            tiles.RemoveAt(0);
            tiles.Add(firstTile);
        }
    }
}
