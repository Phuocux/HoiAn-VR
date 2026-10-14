using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// TEST ONLY:
/// Copies the world-space movement of XROrigin to the boat.
/// Does not control input, paddles, rowing or camera.
/// </summary>
public class BoatFollowXRTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Test")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = false;

    private Vector3 lastXRPosition;
    private Quaternion lastXRRotation;
    private bool initialized;

    private void Start()
    {
        if (xrOrigin == null)
        {
            Debug.LogError(
                "[BoatFollowXRTest] Chưa gán XROrigin.",
                this
            );

            return;
        }

        lastXRPosition = xrOrigin.transform.position;
        lastXRRotation = xrOrigin.transform.rotation;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized || xrOrigin == null)
            return;

        Vector3 positionDelta =
            xrOrigin.transform.position -
            lastXRPosition;

        Quaternion rotationDelta =
            xrOrigin.transform.rotation *
            Quaternion.Inverse(lastXRRotation);

        // ==============================
        // COPY POSITION
        // ==============================

        if (followPosition)
        {
            transform.position += positionDelta;
        }

        // ==============================
        // COPY ROTATION
        // ==============================

        if (followRotation)
        {
            transform.rotation =
                rotationDelta *
                transform.rotation;
        }

        lastXRPosition =
            xrOrigin.transform.position;

        lastXRRotation =
            xrOrigin.transform.rotation;
    }
}