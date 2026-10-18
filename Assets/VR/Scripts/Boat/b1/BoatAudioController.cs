
using UnityEngine;

public class BoatAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatMovement boatMovement;
    [SerializeField] private BoatPaddle leftPaddle;
    [SerializeField] private BoatPaddle rightPaddle;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip paddleSound;
    [SerializeField] private AudioClip boatMovingSound;

    [Header("Paddle Sound")]
    [SerializeField, Min(0f)] private float paddleVolume = 1f;
    [SerializeField, Min(0f)] private float strokeThreshold = 0.15f;
    [SerializeField, Min(0f)] private float strokeSoundCooldown = 0.45f;

    [Header("Boat Movement Sound")]
    [SerializeField, Min(0f)] private float movementVolume = 0.6f;
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;
    [SerializeField, Min(0.01f)] private float volumeFadeSpeed = 2f;

    private AudioSource movementSource;
    private AudioSource paddleSource;

    private float lastLeftStrokeTime = -10f;
    private float lastRightStrokeTime = -10f;

    private float targetMovementVolume;

    private void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length > 0)
            movementSource = sources[0];
        else
            movementSource = gameObject.AddComponent<AudioSource>();

        if (sources.Length > 1)
            paddleSource = sources[1];
        else
            paddleSource = gameObject.AddComponent<AudioSource>();

        movementSource.playOnAwake = false;
        movementSource.loop = true;
        movementSource.spatialBlend = 1f;
        movementSource.volume = 0f;
        movementSource.clip = boatMovingSound;

        paddleSource.playOnAwake = false;
        paddleSource.loop = false;
        paddleSource.spatialBlend = 1f;
        paddleSource.volume = 1f;
    }

    private void Update()
    {
        UpdatePaddleSound(leftPaddle, ref lastLeftStrokeTime);
        UpdatePaddleSound(rightPaddle, ref lastRightStrokeTime);

        UpdateMovementSound();
    }

    private void UpdatePaddleSound(
        BoatPaddle paddle,
        ref float lastStrokeTime)
    {
        if (paddle == null || !paddle.IsHeld)
            return;

        if (paddleSound == null)
            return;

        if (Mathf.Abs(paddle.StrokeInput) < strokeThreshold)
            return;

        if (Time.time - lastStrokeTime < strokeSoundCooldown)
            return;

        paddleSource.PlayOneShot(paddleSound, paddleVolume);
        lastStrokeTime = Time.time;
    }

    private void UpdateMovementSound()
    {
        if (boatMovingSound == null || boatMovement == null)
        {
            targetMovementVolume = 0f;
        }
        else
        {
            float speed = Mathf.Abs(
                boatMovement.GetForwardSpeed()
            );

            targetMovementVolume =
                speed > movementThreshold
                ? movementVolume
                : 0f;
        }

        movementSource.volume = Mathf.MoveTowards(
            movementSource.volume,
            targetMovementVolume,
            volumeFadeSpeed * Time.deltaTime
        );

        if (targetMovementVolume > 0f)
        {
            if (!movementSource.isPlaying)
                movementSource.Play();
        }
        else if (
            movementSource.volume <= 0.001f &&
            movementSource.isPlaying)
        {
            movementSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (movementSource != null)
            movementSource.Stop();

        if (paddleSource != null)
            paddleSource.Stop();
    }
}
