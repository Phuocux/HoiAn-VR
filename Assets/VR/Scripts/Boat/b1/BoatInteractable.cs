using UnityEngine;

/// <summary>
/// Raycast target used to enter the boat.
/// Also controls the visual highlight for pointing interaction.
/// </summary>
public class BoatInteractable : MonoBehaviour
{
    [SerializeField] private BoatMode boatMode;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    public void SetBoatMode(BoatMode mode)
    {
        boatMode = mode;
    }

    public void Interact()
    {
        if (boatMode != null)
            boatMode.TryEnterBoat();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightObject != null)
            highlightObject.SetActive(highlighted);
    }
}
