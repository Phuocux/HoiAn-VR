using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Optional camera look controller for Boat Mode. It does not move the XR
/// Origin; it only handles the simulator's mouse look while VR tracking remains active.
/// </summary>
public class BoatModeLook : MonoBehaviour
{
    [SerializeField] private BoatMode boatMode;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform lookPitchPivot;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;

    private void Update()
    {
        if (boatMode == null || !boatMode.IsInBoat)
            return;

        if (Mouse.current == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        if (xrOrigin != null)
            xrOrigin.Rotate(Vector3.up, delta.x * sensitivity, Space.World);

        if (lookPitchPivot != null)
        {
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minPitch, maxPitch);
            lookPitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
