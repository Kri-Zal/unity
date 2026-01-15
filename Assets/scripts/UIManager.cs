using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public GameObject hudPanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

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
        ShowHUD();
        HideGameOver();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void ShowGameOver(int finalScore, int highScore)
    {
        HideHUD();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    public void ShowHUD()
    {
        if (hudPanel != null)
            hudPanel.SetActive(true);
    }

    public void HideHUD()
    {
        if (hudPanel != null)
            hudPanel.SetActive(false);
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void OnRestartButtonClick()
    {
        GameManager.instance?.RestartGame();
    }
}
