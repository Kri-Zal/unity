using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoryIntroManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject storyPanel;
    public Text storyText;

    [Header("Story Settings")]
    public float typingSpeed = 0.04f;

    [Header("Audio Settings")]
    public AudioClip typingSound;
    [Range(0.1f, 1.0f)]
    public float typingSoundVolume = 0.5f;
    private AudioSource audioSource;

    private const string FIRST_TIME_KEY = "HasSeenStory";
    private const string PLAY_COUNT_KEY = "TimesPlayed";
    private int currentStoryIndex = 0;
    private bool isTyping = false;
    private Button panelButton;
    private string[] storySegments;

    private readonly string[] firstTimeStory = new string[]
    {
        "In a world where cities have become endless concrete labyrinths...\n\n<b>You are ARIA</b>\n\nOne of the last free runners in Neo-Metro City.\n\n<i>[Click anywhere to continue]</i>",
        "The TileManager AI took control of city planning years ago.\n\nThe city now extends forever, with roads and buildings materializing endlessly ahead.\n\n<i>[Click to continue]</i>",
        "The ObstacleSpawner protocol constantly tests citizens.\n\nThose who navigate the urban maze earn their freedom.\n\nThose who stumble must start again.\n\n<i>[Click to continue]</i>",
        "<b>RUN. SURVIVE. SCORE.</b>\n\nHow far can you run before the city claims you?\n\nProve you're the greatest runner Neo-Metro has ever seen.\n\n<i>[Click to start]</i>"
    };

    private readonly string[][] returningPlayerStories = new string[][]
    {
        new string[] {
            "Back for another run, ARIA?\n\nThe city missed you...\n\n<i>[Click to continue]</i>",
            "Time to show these streets what you're made of!\n\n<i>[Click to start]</i>"
        },
        new string[] {
            "Every run makes you stronger.\nEvery fall makes you wiser.\n\n<i>[Click to continue]</i>",
            "Ready to break your record?\n\n<i>[Click to start]</i>"
        },
        new string[] {
            "The Neo-Metro leaderboards await your return...\n\n<i>[Click to continue]</i>",
            "Make them remember your name!\n\n<i>[Click to start]</i>"
        },
        new string[] {
            "The streets are calling, runner...\n\nThey're hungry for speed.\n\n<i>[Click to continue]</i>",
            "Show them what you've got!\n\n<i>[Click to start]</i>"
        }
    };

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (storyPanel == null || storyText == null)
        {
            Debug.LogError("StoryIntroManager: References not assigned in Inspector!");
            return;
        }

        if (typingSound == null)
        {
            Debug.LogWarning("Typing sound not assigned - typewriter effect will be silent");
        }

        SetupStorySegments();

        panelButton = storyPanel.GetComponent<Button>();
        if (panelButton != null)
        {
            panelButton.onClick.AddListener(OnPanelClicked);
        }

        ShowStory();
    }

    void SetupStorySegments()
    {
        int timesPlayed = PlayerPrefs.GetInt(PLAY_COUNT_KEY, 0);
        bool isFirstTime = PlayerPrefs.GetInt(FIRST_TIME_KEY, 0) == 0;

        if (isFirstTime)
        {
            storySegments = firstTimeStory;
            PlayerPrefs.SetInt(FIRST_TIME_KEY, 1);
        }
        else
        {
            int messageSetIndex = Random.Range(0, returningPlayerStories.Length);
            storySegments = returningPlayerStories[messageSetIndex];
        }

        PlayerPrefs.SetInt(PLAY_COUNT_KEY, timesPlayed + 1);
        PlayerPrefs.Save();
    }

    void ShowStory()
    {
        storyPanel.SetActive(true);
        Time.timeScale = 0f;
        currentStoryIndex = 0;
        DisplayCurrentStory();
    }

    void HideStory()
    {
        storyPanel.SetActive(false);
        Time.timeScale = 1f;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void DisplayCurrentStory()
    {
        StartCoroutine(TypeText(storySegments[currentStoryIndex]));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        storyText.text = "";

        foreach (char c in text)
        {
            storyText.text += c;

            if (audioSource != null && typingSound != null && c != ' ' && c != '\n')
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound, typingSoundVolume);
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void OnPanelClicked()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            storyText.text = storySegments[currentStoryIndex];
            isTyping = false;
            return;
        }

        currentStoryIndex++;

        if (currentStoryIndex < storySegments.Length)
        {
            DisplayCurrentStory();
        }
        else
        {
            EndStory();
        }
    }

    void EndStory()
    {
        HideStory();
        StartGame();
    }

    void StartGame()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.enabled = true;
        }
    }

    public void ResetStoryFlag()
    {
        PlayerPrefs.DeleteKey(FIRST_TIME_KEY);
        PlayerPrefs.DeleteKey(PLAY_COUNT_KEY);
        PlayerPrefs.Save();
        Debug.Log("Story and play count reset. Restart to see the initial story.");
    }
}
