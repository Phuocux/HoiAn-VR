using UnityEngine;
using UnityEngine.InputSystem;

public class VRHandControlSwitcher : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    [Header("Tracked Pose Drivers")]
    [SerializeField] private Behaviour leftTracker;
    [SerializeField] private Behaviour rightTracker;

    [Header("Custom Hand Scripts")]
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Custom Hand Start Position")]
    [SerializeField]
    private Vector3 leftHandOffset =
        new Vector3(-0.30f, -0.15f, 0.50f);

    [SerializeField]
    private Vector3 rightHandOffset =
        new Vector3(0.30f, -0.15f, 0.50f);

    private bool customHandMode;

    private void Start()
    {
        SetTrackerMode();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleMode();
        }
    }

    private void ToggleMode()
    {
        if (customHandMode)
        {
            SetTrackerMode();
        }
        else
        {
            SetCustomHandMode();
        }
    }

    private void SetTrackerMode()
    {
        customHandMode = false;

        if (leftHand != null)
            leftHand.CustomMode = false;

        if (rightHand != null)
            rightHand.CustomMode = false;

        if (leftTracker != null)
            leftTracker.enabled = true;

        if (rightTracker != null)
            rightTracker.enabled = true;

        Debug.Log("VR Hand Mode: TRACKER");
    }

    private void SetCustomHandMode()
    {
        customHandMode = true;

        if (leftTracker != null)
            leftTracker.enabled = false;

        if (rightTracker != null)
            rightTracker.enabled = false;

        ResetHandsToCamera();

        if (leftHand != null)
        {
            leftHand.CustomMode = true;
            leftHand.ResetHandPosition();
        }

        if (rightHand != null)
        {
            rightHand.CustomMode = true;
            rightHand.ResetHandPosition();
        }

        Debug.Log("VR Hand Mode: CUSTOM HAND");
    }

    private void ResetHandsToCamera()
    {
        if (cameraTransform == null)
            return;

        if (leftController != null)
        {
            leftController.position =
                cameraTransform.TransformPoint(leftHandOffset);

            leftController.rotation =
                cameraTransform.rotation;
        }

        if (rightController != null)
        {
            rightController.position =
                cameraTransform.TransformPoint(rightHandOffset);

            rightController.rotation =
                cameraTransform.rotation;
        }
    }
}