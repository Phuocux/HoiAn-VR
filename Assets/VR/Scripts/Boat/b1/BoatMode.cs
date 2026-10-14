using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Main state machine for entering/leaving the boat.
/// Disables normal VR locomotion while the player is seated.
/// Also controls the boat interaction prompt.
/// </summary>
[DefaultExecutionOrder(1000)]
public class BoatMode : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private VRSimulationPlayer vrPlayer;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform head;

    [Header("Hands")]
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Boat Points")]
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Interaction")]
    [SerializeField] private BoatInteractable interactable;

    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float promptDistance = 3f;

    public bool IsInBoat { get; private set; }

    private void Awake()
    {
        if (interactable != null)
            interactable.SetBoatMode(this);

        UpdatePrompt();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (IsInBoat) ExitBoat();
            else TryEnterBoat();
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame)
        {
            if (IsInBoat) ExitBoat();
            else TryEnterBoat();
        }

        UpdatePrompt();
    }

    public void TryEnterBoat()
    {
        if (IsInBoat || seatPoint == null || xrOrigin == null)
            return;

        Vector3 playerPosition = xrOrigin.position;
        Vector3 boatPosition = transform.position;

        // Chỉ kiểm tra khoảng cách ngang.
        playerPosition.y = 0f;
        boatPosition.y = 0f;

        float distance =
            Vector3.Distance(
                playerPosition,
                boatPosition);

        // Ngoài vùng tương tác thì không được vào thuyền.
        if (distance > promptDistance)
            return;

        EnterBoat();
    }

    public void EnterBoat()
    {
        if (IsInBoat)
            return;

        IsInBoat = true;

        if (vrPlayer != null)
        {
            vrPlayer.SetBoatLookOnly(true);
        }

        SnapPlayerToPoint(seatPoint);

        // The XR Origin carries the camera and both controllers with the boat.
        if (xrOrigin != null)
            xrOrigin.SetParent(transform, true);

        // Fallback for controllers that are not children of the XR Origin.
        if (leftHand != null)
            leftHand.BeginBoatFollow(transform);

        if (rightHand != null)
            rightHand.BeginBoatFollow(transform);

        UpdatePrompt();
    }

    public void ExitBoat()
    {
        if (!IsInBoat)
            return;

        IsInBoat = false;

        if (leftHand != null)
            leftHand.EndBoatFollow();

        if (rightHand != null)
            rightHand.EndBoatFollow();

        if (xrOrigin != null)
            xrOrigin.SetParent(null, true);

        if (vrPlayer != null)
        {
            vrPlayer.SetBoatLookOnly(false);
        }

        if (exitPoint != null)
            SnapPlayerToPoint(exitPoint);

        UpdatePrompt();
    }

    private void LateUpdate()
    {
        if (!IsInBoat)
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

        if (IsInBoat)
        {
            promptUI.SetActive(true);
            promptText.text = "Ra: F / nút Y";
            return;
        }

        bool nearBoat = false;

        if (xrOrigin != null)
        {
            Vector3 playerPosition = xrOrigin.position;
            Vector3 boatPosition = transform.position;
            playerPosition.y = 0f;
            boatPosition.y = 0f;
            nearBoat = Vector3.Distance(playerPosition, boatPosition) <= promptDistance;
        }

        promptUI.SetActive(nearBoat);

        if (nearBoat)
            promptText.text = "Nhấn F / nút Y";
    }

    private void SnapPlayerToPoint(Transform point)
    {
        if (xrOrigin == null || point == null)
            return;

        Vector3 headOffset = head != null
            ? head.position - xrOrigin.position
            : Vector3.zero;

        headOffset.y = 0f;

        xrOrigin.position = point.position - headOffset;

        Vector3 flatForward = Vector3.ProjectOnPlane(point.forward, Vector3.up);
        if (flatForward.sqrMagnitude > 0.0001f)
            xrOrigin.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
    }
}
