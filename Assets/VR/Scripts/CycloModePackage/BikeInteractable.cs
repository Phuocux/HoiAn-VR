using UnityEngine;

/// <summary>Interaction target used to enter Cyclo mode.</summary>
public class BikeInteractable : MonoBehaviour
{
    [SerializeField] private BikeMode bikeMode;
    [SerializeField] private GameObject highlightObject;

    public void SetBikeMode(BikeMode mode) => bikeMode = mode;

    public void Interact()
    {
        if (bikeMode != null)
            bikeMode.TryEnterBike();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightObject != null)
            highlightObject.SetActive(highlighted);
    }
}
