using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovementFull : MonoBehaviour
{
    [Header("Forward & Lane Settings")]
    public float forwardSpeed = 10f;
    public float laneDistance = 5f;
    public float laneChangeSpeed = 8f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Swipe Detection")]
    public float swipeThreshold = 50f;

    [Header("Difficulty Settings")]
    public float speedIncreaseRate = 0.1f;  // Speed increase per second
    public float maxForwardSpeed = 25f;    // Maximum speed cap
    public float speedIncreaseDelay = 5f;   // Wait before increasing speed
    private float timeSinceStart = 0f;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip gameOverSound;
    [Range(0.1f, 1.0f)]
    public float jumpVolume = 0.7f;
    [Range(0.1f, 1.0f)]
    public float gameOverVolume = 1.0f;

    [Header("Debug")]
    public bool debugLogs = false;

    private CharacterController controller;
    private AudioSource audioSource;
    private int currentLane = 1;
    private float verticalVelocity = -2f;
    private bool isStumbling = false;
    private float currentForwardSpeed;

    private Vector2 touchStart, mouseStart;
    private bool wasTouchPressed = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("[PlayerMovementFull] CharacterController missing!");
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("[PlayerMovementFull] Animator component not found!");
        }

        verticalVelocity = -2f;
        currentForwardSpeed = forwardSpeed;
        timeSinceStart = 0f;

        // Add AudioSource if not present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (animator)
        {
            animator.Rebind();
            animator.ResetTrigger("Jump");
            animator.Update(0f);
            animator.Play("Running (1)", 0, 0f);
        }
    }

    void Update()
    {
        if (!isStumbling)
        {
            HandleKeyboard();
            HandleMouseSwipe();
            HandleTouchSwipe();
            UpdateDifficulty();
        }

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        float targetX = (currentLane - 1) * laneDistance;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float lateralDelta = newX - transform.position.x;

        float forwardDelta = currentForwardSpeed * Time.deltaTime;
        Vector3 worldMove = new Vector3(lateralDelta, verticalVelocity * Time.deltaTime, forwardDelta);

        controller.Move(worldMove);

        // Update animator run speed if available
        if (animator && !isStumbling)
        {
            animator.SetFloat("RunSpeed", currentForwardSpeed / forwardSpeed);
        }
    }

    void UpdateDifficulty()
    {
        timeSinceStart += Time.deltaTime;

        // Start increasing speed after delay
        if (timeSinceStart > speedIncreaseDelay)
        {
            float targetSpeed = forwardSpeed + ((timeSinceStart - speedIncreaseDelay) * speedIncreaseRate);
            currentForwardSpeed = Mathf.Min(targetSpeed, maxForwardSpeed);

            if (debugLogs && Mathf.Floor(timeSinceStart) % 5 == 0) // Log every 5 seconds
            {
                Debug.Log($"Speed increased to: {currentForwardSpeed:F1}");
            }
        }
    }

    void HandleKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
            MoveLane(-1);
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
            MoveLane(1);

        if (kb.spaceKey.wasPressedThisFrame)
            Jump();
    }

    void HandleMouseSwipe()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            mouseStart = mouse.position.ReadValue();

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            Vector2 mouseEnd = mouse.position.ReadValue();
            Vector2 delta = mouseEnd - mouseStart;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (Mathf.Abs(delta.x) > swipeThreshold)
                    MoveLane(delta.x > 0 ? 1 : -1);
            }
            else if (Mathf.Abs(delta.y) > swipeThreshold && delta.y > 0)
            {
                Jump();
            }
        }
    }

    void HandleTouchSwipe()
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        var t = ts.primaryTouch;
        if (t == null) return;

        bool pressed = t.press.isPressed;

        if (pressed && !wasTouchPressed)
            touchStart = t.position.ReadValue();
        else if (!pressed && wasTouchPressed)
        {
            Vector2 end = t.position.ReadValue();
            Vector2 delta = end - touchStart;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (Mathf.Abs(delta.x) > swipeThreshold)
                    MoveLane(delta.x > 0 ? 1 : -1);
            }
            else if (Mathf.Abs(delta.y) > swipeThreshold && delta.y > 0)
            {
                Jump();
            }
        }

        wasTouchPressed = pressed;
    }

    void MoveLane(int direction)
    {
        int target = Mathf.Clamp(currentLane + direction, 0, 2);
        if (target != currentLane)
        {
            currentLane = target;
            if (debugLogs) Debug.Log("Lane -> " + currentLane);
        }
    }

    void Jump()
    {
        if (!controller.isGrounded) return;

        verticalVelocity = jumpForce;

        // Play jump sound
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }

        if (animator)
        {
            animator.SetTrigger("Jump");
        }

        if (debugLogs) Debug.Log
        ("Jump triggered, vertVel="
         + verticalVelocity);
    }

    public void TriggerStumble()
    {
        if (isStumbling) return;

        isStumbling = true;
        currentForwardSpeed = 0f;

        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot
            (gameOverSound, gameOverVolume);
        }

        if (animator)
        {
            animator.SetTrigger("Stumble");
        }

        if (debugLogs) Debug.Log("Player stumbling!");
    }

    public void ResetDifficulty()
    {
        timeSinceStart = 0f;
        currentForwardSpeed = forwardSpeed;
        isStumbling = false;

        if (animator)
        {
            animator.SetFloat("RunSpeed", 1f);
            animator.ResetTrigger("Stumble");
            animator.ResetTrigger("Jump");
        }
    }
}
