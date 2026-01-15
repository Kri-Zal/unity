using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Score Settings")]
    public Transform player;
    public int pointsPerMeter = 10;
    public float difficultyRamp = 0.8f;  // How quickly scoring gets harder

    private int currentScore = 0;
    private int highScore = 0;
    private float startZ;
    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player != null)
        {
            startZ = player.position.z;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (!isGameActive || player == null) return;

        float distanceTraveled = player.position.z - startZ;
        float difficulty = 1f + (distanceTraveled / 100f);
        float adjustedPoints = pointsPerMeter / Mathf.Pow(difficulty, difficultyRamp);
        currentScore = Mathf.FloorToInt(distanceTraveled * adjustedPoints);

        UIManager.instance?.UpdateScore(currentScore);
    }

    public void StopScoring()
    {
        isGameActive = false;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;
}
