using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Enters/exits Cyclo mode using the same general pattern as BoatMode.
/// Normal player locomotion is disabled while riding.
/// </summary>
[DefaultExecutionOrder(999)]
public class BikeMode : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private VRSimulationPlayer vrPlayer;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform head;

    [Header("Hands")]
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Bike Points")]
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Bike Systems")]
    [SerializeField] private BikeHandlebar bikeHandlebar;
    [SerializeField] private BikeMovement bikeMovement;
    [SerializeField] private BikeInteractable interactable;
    [SerializeField] private BoatMode boatMode;

    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;
    [SerializeField, Min(0.1f)] private float promptDistance = 3f;

    public bool IsOnBike { get; private set; }
    public static bool AnyBikeModeActive { get; private set; }
    private static int lastToggleInputFrame = -1;
    public static bool ConsumedToggleInputThisFrame => lastToggleInputFrame == Time.frameCount;

    private bool leftCustomModeBeforeBike;
    private bool rightCustomModeBeforeBike;

    private void Awake()
    {
        if (interactable != null)
            interactable.SetBikeMode(this);

        UpdatePrompt();
    }

    private void Update()
    {
        // Do not let the bike steal the enter/exit input while riding a boat.
        if (boatMode == null || !boatMode.IsInBoat)
        {
            bool keyboardToggle = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
            bool gamepadToggle = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;

            if (keyboardToggle || gamepadToggle)
            {
                if (IsOnBike)
                {
                    lastToggleInputFrame = Time.frameCount;
                    ExitBike();
                }
                else
                {
                    TryEnterBike();
                    if (IsOnBike)
                        lastToggleInputFrame = Time.frameCount;
                }
            }
        }

        UpdatePrompt();
    }

    public void TryEnterBike()
    {
        if (IsOnBike || seatPoint == null || xrOrigin == null)
            return;

        if (boatMode != null && boatMode.IsInBoat)
            return;

        Vector3 playerPosition = xrOrigin.position;
        Vector3 bikePosition = transform.position;
        playerPosition.y = 0f;
        bikePosition.y = 0f;

        if (Vector3.Distance(playerPosition, bikePosition) > promptDistance)
            return;

        EnterBike();
    }

    public void EnterBike()
    {
        if (IsOnBike || seatPoint == null)
            return;

        // Save the player's normal hand poses before moving the XR Origin to the seat.
        // ExitBike restores these camera-relative poses when the player gets off.
        if (leftHand != null)
            leftHand.ResetHandPosition();
        if (rightHand != null)
            rightHand.ResetHandPosition();

        IsOnBike = true;
        AnyBikeModeActive = true;

        if (leftHand != null)
        {
            leftCustomModeBeforeBike = leftHand.CustomMode;
            leftHand.CustomMode = true;
        }
        if (rightHand != null)
        {
            rightCustomModeBeforeBike = rightHand.CustomMode;
            rightHand.CustomMode = true;
        }

        if (vrPlayer != null)
            vrPlayer.enabled = false;

        SnapPlayerToPoint(seatPoint);

        if (xrOrigin != null)
            xrOrigin.SetParent(transform, true);

        // Fallback for simulated hands that are not children of XR Origin.
        if (leftHand != null)
            leftHand.BeginBoatFollow(transform);
        if (rightHand != null)
            rightHand.BeginBoatFollow(transform);

        if (bikeHandlebar != null)
            bikeHandlebar.BeginBikeMode();

        if (bikeMovement != null)
            bikeMovement.ResetSpeed();

        UpdatePrompt();
    }

    public void ExitBike()
    {
        if (!IsOnBike)
            return;

        if (bikeHandlebar != null)
            bikeHandlebar.EndBikeMode();

        if (leftHand != null)
        {
            leftHand.EndBikeHold();
            leftHand.EndBoatFollow();
        }
        if (rightHand != null)
        {
            rightHand.EndBikeHold();
            rightHand.EndBoatFollow();
            rightHand.CustomMode = rightCustomModeBeforeBike;
        }
        if (leftHand != null)
            leftHand.CustomMode = leftCustomModeBeforeBike;

        IsOnBike = false;
        AnyBikeModeActive = false;

        if (xrOrigin != null)
            xrOrigin.SetParent(null, true);

        if (bikeMovement != null)
            bikeMovement.ResetSpeed();

        if (exitPoint != null)
            SnapPlayerToPoint(exitPoint);

        if (vrPlayer != null)
            vrPlayer.enabled = true;

        // Release the visible hands from the handlebar and restore their normal
        // camera-relative positions/rotations after the player has been repositioned.
        if (leftHand != null)
            leftHand.ResetHandPose();
        if (rightHand != null)
            rightHand.ResetHandPose();

        UpdatePrompt();
    }

    private void LateUpdate()
    {
        if (!IsOnBike)
            return;

        if (leftHand != null)
            leftHand.ApplyBoatFollow();
        if (rightHand != null)
            rightHand.ApplyBoatFollow();
    }

    private void UpdatePrompt()
    {
        if (promptUI == null || promptText == null)
            return;

        if (IsOnBike)
        {
            promptUI.SetActive(true);
            promptText.text = "Xuống xe: F / nút Y";
            return;
        }

        if (boatMode != null && boatMode.IsInBoat)
        {
            promptUI.SetActive(false);
            return;
        }

        bool nearBike = false;
        if (xrOrigin != null)
        {
            Vector3 playerPosition = xrOrigin.position;
            Vector3 bikePosition = transform.position;
            playerPosition.y = 0f;
            bikePosition.y = 0f;
            nearBike = Vector3.Distance(playerPosition, bikePosition) <= promptDistance;
        }

        promptUI.SetActive(nearBike);
        if (nearBike)
            promptText.text = "Lên xe: F / nút Y";
    }

    private void SnapPlayerToPoint(Transform point)
    {
        if (xrOrigin == null || point == null)
            return;

        Vector3 headOffset = head != null ? head.position - xrOrigin.position : Vector3.zero;
        headOffset.y = 0f;
        xrOrigin.position = point.position - headOffset;

        Vector3 flatForward = Vector3.ProjectOnPlane(point.forward, Vector3.up);
        if (flatForward.sqrMagnitude > 0.0001f)
            xrOrigin.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
    }
}
