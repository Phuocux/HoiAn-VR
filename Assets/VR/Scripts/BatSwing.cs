using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BatSwing : MonoBehaviour
{
    [SerializeField] private Transform swingRoot;
    [SerializeField] private TrailRenderer swingTrail;

    [SerializeField] private float windUpAngle = -25f;
    [SerializeField] private float hitAngle = 65f;

    [SerializeField] private float windUpDuration = 0.15f;
    [SerializeField] private float hitDuration = 0.12f;
    [SerializeField] private float returnDuration = 0.2f;

    public bool IsStriking { get; private set; }

    private Quaternion restRotation;
    private bool isSwinging;

    private void Awake()
    {
        if (swingRoot != null)
            restRotation = swingRoot.localRotation;

        if (swingTrail != null)
        {
            swingTrail.emitting = false;
            swingTrail.Clear();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.lKey.wasPressedThisFrame &&
            !isSwinging &&
            swingRoot != null)
        {
            StartCoroutine(Swing());
        }
    }

    private IEnumerator Swing()
    {
        isSwinging = true;

        // Đưa gậy về phía sau.
        yield return RotateBetween(
            0f, windUpAngle, windUpDuration
        );

        // Chỉ đoạn vung tới mới tính là cú đánh.
        IsStriking = true;

        if (swingTrail != null)
        {
            swingTrail.Clear();
            swingTrail.emitting = true;
        }

        yield return RotateBetween(
            windUpAngle, hitAngle, hitDuration
        );

        IsStriking = false;

        if (swingTrail != null)
            swingTrail.emitting = false;

        // Trả gậy về tư thế cầm.
        yield return RotateBetween(
            hitAngle, 0f, returnDuration
        );

        isSwinging = false;
    }

    private IEnumerator RotateBetween(
        float from,
        float to,
        float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float angle = Mathf.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / duration)
            );

            swingRoot.localRotation =
                restRotation * Quaternion.Euler(angle, 0f, 0f);

            yield return null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        IsStriking = false;
        isSwinging = false;

        if (swingRoot != null)
            swingRoot.localRotation = restRotation;

        if (swingTrail != null)
        {
            swingTrail.emitting = false;
            swingTrail.Clear();
        }
    }
}