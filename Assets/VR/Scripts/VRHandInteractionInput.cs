using UnityEngine;
using UnityEngine.InputSystem;

public class VRHandInteractionInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference gripAction;
    [SerializeField] private InputActionReference triggerAction;

    public bool GripPressed =>
        gripAction != null &&
        gripAction.action.IsPressed();

    public bool TriggerPressed =>
        triggerAction != null &&
        triggerAction.action.IsPressed();

    private void OnEnable()
    {
        if (gripAction != null)
            gripAction.action.Enable();

        if (triggerAction != null)
            triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        if (gripAction != null)
            gripAction.action.Disable();

        if (triggerAction != null)
            triggerAction.action.Disable();
    }
}