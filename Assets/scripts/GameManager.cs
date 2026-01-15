using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    private AudioSource audioSource;

    private bool isGameOver = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  
            // Keep music playing between scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure audio source for background music
        audioSource.clip = backgroundMusic;
        audioSource.volume = musicVolume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Start playing background music
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.Play();
        }
        else if (backgroundMusic == null)
        {
            Debug.LogWarning("Background music clip not assigned!");
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        // Optionally fade out music
        StartCoroutine(FadeOutMusic(1.5f));

        yield return new WaitForSeconds(1.5f);

        ScoreManager.instance?.StopScoring();

        int finalScore = ScoreManager.instance?.GetCurrentScore() ?? 0;
        int highScore = ScoreManager.instance?.GetHighScore() ?? 0;

        UIManager.instance?.ShowGameOver(finalScore, highScore);

        Time.timeScale = 0f;
        Debug.Log("GAME OVER!");
    }

    IEnumerator FadeOutMusic(float duration)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = 
            Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        audioSource.Stop();
    }

    public void RestartGame()
    {
        // Reset audio
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
            audioSource.Play();
        }

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.
        SceneManager.GetActiveScene().name
        );
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
