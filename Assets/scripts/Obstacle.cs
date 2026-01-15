using UnityEngine;
public class Obstacle : MonoBehaviour
{    private const float DESPAWN_Z_OFFSET = -10f;
    private Transform playerTransform;
    private ObstacleSpawner spawner;

    public void Initialize(Transform player, ObstacleSpawner obstacleSpawner)
    {
        playerTransform = player;
        spawner = obstacleSpawner;
    }
    void Update()
    {
        if (playerTransform != null && transform.position.z < playerTransform.position.z + DESPAWN_Z_OFFSET)
        {
            ReturnToPool();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit obstacle: " + gameObject.name);

            PlayerMovementFull player = other.GetComponent<PlayerMovementFull>();
            if (player != null)
            {
                player.TriggerStumble();
            }

            GameManager.instance?.TriggerGameOver();
        }
    }
    void ReturnToPool()
    {
        if (spawner != null)
        {
            spawner.ReturnObstacle(this);
        }
    }
}
