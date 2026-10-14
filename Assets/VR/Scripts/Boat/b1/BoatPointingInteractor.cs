using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Points from the VR simulation hand controllers and enters the boat when
/// the configured interaction input is pressed.
/// </summary>
public class BoatPointingInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform leftHandController;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private BoatInteractable boat;

    [Header("Ray")]
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("Input")]
    [SerializeField] private bool useLeftTrigger = true;
    [SerializeField] private bool useRightTrigger = true;

    [Header("Grip")]
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    private bool leftPointingAtBoat;
    private bool rightPointingAtBoat;

    private void Update()
    {
        if (boat == null)
            return;

        leftPointingAtBoat = IsPointingAtBoat(leftHandController);
        rightPointingAtBoat = IsPointingAtBoat(rightHandController);

        boat.SetHighlighted(leftPointingAtBoat || rightPointingAtBoat);

        if (TryGetPointingHand(out Transform hand, out bool left))
        {
            Ray ray = new Ray(hand.position, hand.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers, QueryTriggerInteraction.Ignore))
            {
                BoatInteractable target = hit.collider.GetComponentInParent<BoatInteractable>();
                if (target == boat)
                {
                    bool pressed = left
                        ? (useLeftTrigger && IsLeftTriggerPressed())
                        : (useRightTrigger && IsRightTriggerPressed());

                    if (pressed)
                        target.Interact();
                }
            }
        }

        if (leftPointingAtBoat && leftHand != null && leftHand.GripPressed)
            boat.Interact();

        if (rightPointingAtBoat && rightHand != null && rightHand.GripPressed)
            boat.Interact();
    }

    private bool TryGetPointingHand(out Transform hand, out bool left)
    {
        hand = null;
        left = false;

        bool leftValid = leftHandController != null;
        bool rightValid = rightHandController != null;

        if (!leftValid && !rightValid)
            return false;

        if (leftValid)
        {
            hand = leftHandController;
            left = true;
        }

        if (rightValid && (hand == null))
        {
            hand = rightHandController;
            left = false;
        }

        // Prefer the hand whose ray is currently closer to the boat.
        if (leftValid && rightValid)
        {
            float leftAngle = Vector3.Angle(leftHandController.forward, boat.transform.position - leftHandController.position);
            float rightAngle = Vector3.Angle(rightHandController.forward, boat.transform.position - rightHandController.position);

            if (rightAngle < leftAngle)
            {
                hand = rightHandController;
                left = false;
            }
        }

        return hand != null;
    }

    private bool IsLeftTriggerPressed()
    {
        return Gamepad.current != null && Gamepad.current.leftTrigger.isPressed;
    }

    private bool IsRightTriggerPressed()
    {
        return Gamepad.current != null && Gamepad.current.rightTrigger.isPressed;
    }

    private bool IsPointingAtBoat(Transform hand)
    {
        if (hand == null || boat == null)
            return false;

        Ray ray = new Ray(hand.position, hand.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers, QueryTriggerInteraction.Ignore))
            return false;

        BoatInteractable target = hit.collider.GetComponentInParent<BoatInteractable>();
        return target == boat;
    }
}
