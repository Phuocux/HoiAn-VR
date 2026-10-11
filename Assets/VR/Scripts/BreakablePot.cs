using System.Collections;
using UnityEngine;

public class BreakablePot : MonoBehaviour
{
    [SerializeField] private DapNieuGameManager gameManager;
    [SerializeField] private int points = 10;

    [Header("Break Effect")]
    [SerializeField] private ParticleSystem breakEffect;
    [SerializeField] private Transform breakPoint;

    [Header("Swing")]
    [SerializeField] private Transform swingPivot;
    [SerializeField] private float swingAngle = 12f;
    [SerializeField] private float swingDuration = 1.2f;

    [Header("Touch Sound")]
    [SerializeField] private AudioSource touchAudioSource;
    [SerializeField] private AudioClip touchSound;
    [SerializeField] private float touchCooldown = 0.4f;

    private bool isBroken;
    private float nextTouchTime;
    private Quaternion restRotation;
    private Coroutine swingRoutine;

    private void Awake()
    {
        if (swingPivot != null)
            restRotation = swingPivot.localRotation;
    }

    public void ReceiveHit(float speed, float breakSpeed)
    {
        if (isBroken || gameManager == null)
            return;

        if (!gameManager.IsPlaying)
            return;

        if (speed >= breakSpeed)
        {
            Break();
            return;
        }

        if (Time.time < nextTouchTime)
            return;

        nextTouchTime = Time.time + touchCooldown;

        if (touchAudioSource != null && touchSound != null)
            touchAudioSource.PlayOneShot(touchSound);

        if (swingPivot != null)
        {
            if (swingRoutine != null)
                StopCoroutine(swingRoutine);

            swingRoutine = StartCoroutine(Swing());
        }
    }

    private IEnumerator Swing()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, swingDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            float angle =
                Mathf.Sin(progress * Mathf.PI * 6f)
                * swingAngle
                * (1f - progress);

            swingPivot.localRotation =
                restRotation * Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        swingPivot.localRotation = restRotation;
        swingRoutine = null;
    }

    public void Break()
    {
        if (isBroken || gameManager == null)
            return;

        if (!gameManager.IsPlaying)
            return;

        isBroken = true;

        if (breakEffect != null)
        {
            Vector3 position = breakPoint != null
                ? breakPoint.position
                : transform.position;

            Instantiate(
                breakEffect,
                position,
                Quaternion.identity
            );
        }

        gameManager.AddScore(points);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (swingRoutine != null)
            StopCoroutine(swingRoutine);

        swingRoutine = null;

        if (swingPivot != null)
            swingPivot.localRotation = restRotation;
    }

    public void ResetPot()
    {
        isBroken = false;
        nextTouchTime = 0f;

        if (swingPivot != null)
            swingPivot.localRotation = restRotation;

        gameObject.SetActive(true);
    }
}