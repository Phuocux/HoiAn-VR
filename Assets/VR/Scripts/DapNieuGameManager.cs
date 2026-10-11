using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DapNieuGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float duration = 60f;
    [SerializeField] private BreakablePot[] pots;
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip potBreakSound;

    public int Score { get; private set; }
    public bool IsPlaying { get; private set; }

    private float timeRemaining;
    private bool hasPlayed;
    private int potsRemaining;
    private bool wonRound;

    private void Start()
    {
        Score = 0;
        IsPlaying = false;
        hasPlayed = false;

        UpdateDisplay();
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            bool gamepadPressed =
                Gamepad.current != null &&
                Gamepad.current.buttonSouth.wasPressedThisFrame;

            bool keyboardPressed =
                Keyboard.current != null &&
                Keyboard.current.enterKey.wasPressedThisFrame;

            if (gamepadPressed || keyboardPressed)
                StartRound();

            return;
        }

        timeRemaining = Mathf.Max(
            0f,
            timeRemaining - Time.deltaTime
        );

        if (timeRemaining <= 0f)
            IsPlaying = false;

        UpdateDisplay();
    }

    public void StartRound()
    {
        if (IsPlaying)
            return;

        Score = 0;
        timeRemaining = Mathf.Max(0f, duration);

        potsRemaining = 0;
        wonRound = false;

        foreach (BreakablePot pot in pots)
        {
            if (pot == null)
                continue;

            pot.ResetPot();
            potsRemaining++;
        }

        hasPlayed = true;
        IsPlaying = timeRemaining > 0f;

        UpdateDisplay();
    }

    public void AddScore(int points)
    {
        if (!IsPlaying)
            return;

        Score += points;
        potsRemaining--;

        if (hitAudioSource != null && potBreakSound != null)
            hitAudioSource.PlayOneShot(potBreakSound);

        if (potsRemaining <= 0)
        {
            wonRound = true;
            IsPlaying = false;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null)
            scoreText.text = "Điểm: " + Score;

        if (timerText == null)
            return;

        if (IsPlaying)
        {
            timerText.text =
                "Thời gian: " + Mathf.CeilToInt(timeRemaining);
        }
        else
        {
            if (!hasPlayed)
            {
                timerText.text = "ESC để trở về màn chính/Enter để bắt đầu";
            }
            else if (wonRound)
            {
                timerText.text =
                    "Đã đập hết niêu!Enter để chơi lại";
            }
            else
            {
                timerText.text =
                    "Hết giờ! Enter để chơi lại";
            }
        }
    }
}