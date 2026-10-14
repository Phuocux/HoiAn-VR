using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Mouse and D-Pad camera look while the normal player controller is disabled.</summary>
public class BikeModeLook : MonoBehaviour
{
    [SerializeField] private BikeMode bikeMode;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform lookPitchPivot;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField, Min(1f)] private float dpadLookSpeed = 60f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;
    private Quaternion basePitchRotation = Quaternion.identity;

    private void Awake()
    {
        if (lookPitchPivot != null)
        {
            basePitchRotation = lookPitchPivot.localRotation;
            pitch = 0f;
        }
    }

    private void Update()
    {
        if (bikeMode == null || !bikeMode.IsOnBike)
            return;

        Vector2 mouse = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        Vector2 dpad = Gamepad.current != null ? Gamepad.current.dpad.ReadValue() : Vector2.zero;

        float yaw = mouse.x * mouseSensitivity + dpad.x * dpadLookSpeed * Time.deltaTime;
        float pitchDelta = -mouse.y * mouseSensitivity - dpad.y * dpadLookSpeed * Time.deltaTime;

        if (xrOrigin != null && Mathf.Abs(yaw) > 0.0001f)
            xrOrigin.Rotate(Vector3.up, yaw, Space.World);

        if (lookPitchPivot != null)
        {
            pitch = Mathf.Clamp(pitch + pitchDelta, minPitch, maxPitch);
            lookPitchPivot.localRotation = basePitchRotation * Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
