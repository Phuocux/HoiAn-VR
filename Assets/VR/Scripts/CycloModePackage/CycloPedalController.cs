using UnityEngine;

/// <summary>
/// Rotates one pedal model around its LOCAL Z axis while the cyclo is moving.
/// Optionally loops a pedal/chain sound while the cyclo is moving.
/// Attach this component to the Cyclo root or another always-active object.
/// </summary>
public class CycloPedalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BikeMovement bikeMovement;

    [Header("Single Pedal Model")]
    [Tooltip("The one pedal model or its pivot. Its local origin should be at the rotation center.")]
    [SerializeField] private Transform pedalTransform;
    [Tooltip("Full pedal rotations per metre travelled. Adjust to taste.")]
    [SerializeField, Min(0f)] private float rotationsPerMetre = 1.5f;
    [Tooltip("Enable this if the pedal spins in the wrong direction.")]
    [SerializeField] private bool reverseRotation;

    [Header("Pedal Sound")]
    [Tooltip("Optional loop sound while the cyclo is moving. Leave empty to disable sound.")]
    [SerializeField] private AudioClip pedalLoopClip;
    [Tooltip("Optional AudioSource. If empty, the script finds or creates one when a clip is assigned.")]
    [SerializeField] private AudioSource pedalAudioSource;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.4f;
    [Tooltip("Sound stops when speed is at or below this value.")]
    [SerializeField, Min(0f)] private float soundStartSpeed = 0.05f;
    [SerializeField, Min(0.1f)] private float minimumPitch = 0.8f;
    [Tooltip("Additional pitch for each unit of vehicle speed.")]
    [SerializeField, Min(0f)] private float pitchPerSpeed = 0.15f;

    private void Awake()
    {
        if (bikeMovement == null)
            bikeMovement = GetComponent<BikeMovement>();

        if (pedalAudioSource == null)
            pedalAudioSource = GetComponent<AudioSource>();

        if (pedalAudioSource == null && pedalLoopClip != null)
            pedalAudioSource = gameObject.AddComponent<AudioSource>();

        if (pedalAudioSource != null)
        {
            pedalAudioSource.playOnAwake = false;
            pedalAudioSource.loop = true;
            pedalAudioSource.volume = soundVolume;
            if (pedalLoopClip != null)
                pedalAudioSource.clip = pedalLoopClip;
        }
    }

    private void Update()
    {
        float speed = bikeMovement != null ? Mathf.Max(0f, bikeMovement.CurrentSpeed) : 0f;

        if (pedalTransform != null && speed > 0f)
        {
            float direction = reverseRotation ? -1f : 1f;
            float degrees = speed * rotationsPerMetre * 360f * Time.deltaTime * direction;
            // Rotate only around the pedal model's own LOCAL Z axis.
            pedalTransform.Rotate(Vector3.up, degrees, Space.Self);
        }

        UpdatePedalSound(speed);
    }

    private void UpdatePedalSound(float speed)
    {
        if (pedalAudioSource == null || pedalLoopClip == null)
            return;

        pedalAudioSource.volume = soundVolume;
        bool shouldPlay = speed > soundStartSpeed;

        if (shouldPlay)
        {
            pedalAudioSource.pitch = minimumPitch + speed * pitchPerSpeed;
            if (!pedalAudioSource.isPlaying)
            {
                pedalAudioSource.clip = pedalLoopClip;
                pedalAudioSource.loop = true;
                pedalAudioSource.Play();
            }
        }
        else if (pedalAudioSource.isPlaying)
        {
            pedalAudioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (pedalAudioSource != null && pedalAudioSource.isPlaying)
            pedalAudioSource.Stop();
    }
}
