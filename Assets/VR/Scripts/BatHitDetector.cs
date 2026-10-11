using UnityEngine;

public class BatHitDetector : MonoBehaviour
{
    [SerializeField] private float minimumTouchSpeed = 0.05f;
    [SerializeField] private float breakSpeed = 1.2f;
    [SerializeField] private BatSwing batSwing;

    private Vector3 previousPosition;
    private float tipSpeed;

    private void OnEnable()
    {
        previousPosition = transform.position;
        tipSpeed = 0f;
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;

        tipSpeed = Vector3.Distance(
            currentPosition,
            previousPosition
        ) / Time.fixedDeltaTime;

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (tipSpeed < minimumTouchSpeed)
            return;

        BreakablePot pot =
            other.GetComponentInParent<BreakablePot>();

        if (pot == null)
            return;

        bool striking =
            batSwing != null && batSwing.IsStriking;

        // Ngoài cú đánh, chỉ cho phản ứng lắc.
        float requiredSpeed = striking
            ? breakSpeed
            : float.PositiveInfinity;

        pot.ReceiveHit(tipSpeed, requiredSpeed);
    }
}