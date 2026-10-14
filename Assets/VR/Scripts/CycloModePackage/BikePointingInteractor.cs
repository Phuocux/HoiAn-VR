using UnityEngine;

/// <summary>Allows entering the bike by pointing a simulated hand at it and pressing Trigger or Grip.</summary>
public class BikePointingInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftHandController;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private BikeInteractable bike;
    [SerializeField] private BikeMode bikeMode;
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Ray")]
    [SerializeField, Min(0.1f)] private float maxDistance = 8f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private void Update()
    {
        if (bike == null)
            return;

        // Button East is the accelerator while riding; do not treat it as a pointing trigger then.
        if (bikeMode != null && bikeMode.IsOnBike)
        {
            bike.SetHighlighted(false);
            return;
        }

        bool leftAims = IsPointingAtBike(leftHandController);
        bool rightAims = IsPointingAtBike(rightHandController);
        bike.SetHighlighted(leftAims || rightAims);

        if (leftAims && leftHand != null && (leftHand.TriggerPressed || leftHand.GripPressed))
            bike.Interact();
        else if (rightAims && rightHand != null && (rightHand.TriggerPressed || rightHand.GripPressed))
            bike.Interact();
    }

    private bool IsPointingAtBike(Transform hand)
    {
        if (hand == null || bike == null)
            return false;

        Ray ray = new Ray(hand.position, hand.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers, QueryTriggerInteraction.Ignore))
            return false;

        return hit.collider.GetComponentInParent<BikeInteractable>() == bike;
    }
}
