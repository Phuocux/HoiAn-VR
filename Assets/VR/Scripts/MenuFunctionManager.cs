
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MenuFunctionManager : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject menuCanvas;

    [Header("Menu Toggle")]
    [SerializeField] private bool allowEscape = true;
    [SerializeField] private bool allowGamepadStart = true;

    [Tooltip("Gán Input Action riêng cho nút Quest nếu đã tạo và xác nhận không bị dùng.")]
    [SerializeField] private InputActionReference questMenuToggleAction;

    [Header("Teleport")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform teleportPoint1;
    [SerializeField] private Transform teleportPoint2;
    [SerializeField] private Transform teleportPoint3;

    private void OnEnable()
    {
        if (questMenuToggleAction != null)
            questMenuToggleAction.action?.Enable();
    }

    private void OnDisable()
    {
        if (questMenuToggleAction != null)
            questMenuToggleAction.action?.Disable();
    }

    private void Update()
    {
        bool toggle = false;

        if (allowEscape &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            toggle = true;
        }

        if (allowGamepadStart &&
            Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame)
        {
            toggle = true;
        }

        if (questMenuToggleAction != null &&
            questMenuToggleAction.action != null &&
            questMenuToggleAction.action.WasPressedThisFrame())
        {
            toggle = true;
        }

        if (toggle)
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (menuCanvas == null)
        {
            Debug.LogError("[MenuFunctionManager] Chưa gán Menu Canvas.", this);
            return;
        }

        menuCanvas.SetActive(!menuCanvas.activeSelf);
    }

    public void OpenMenu()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(true);
    }

    public void CloseMenu()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }

    public void ExitApplication()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("ExitApplication: lệnh thoát hoạt động trong bản build; Editor vẫn tiếp tục Play Mode.");
#endif
    }

    public void TeleportToP1() => TeleportTo(teleportPoint1);
    public void TeleportToP2() => TeleportTo(teleportPoint2);
    public void TeleportToP3() => TeleportTo(teleportPoint3);

    private void TeleportTo(Transform destination)
    {
        if (playerRig == null || destination == null)
        {
            Debug.LogError(
                "[MenuFunctionManager] Chưa gán Player Rig hoặc điểm Teleport.",
                this);
            return;
        }

        playerRig.position = destination.position;

        Vector3 currentRotation = playerRig.eulerAngles;
        playerRig.rotation = Quaternion.Euler(
            currentRotation.x,
            destination.eulerAngles.y,
            currentRotation.z);
    }
}
