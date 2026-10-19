using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DapNieuEntrance : MonoBehaviour
{
    [SerializeField] private Transform playerHead;
    [SerializeField] private GameObject prompt;

    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private float maximumHeightDifference = 3f;

    private const string ScenePath =
        "Assets/Scenes/HoiAnMap/DapNieuScene.unity";

    private bool isLoading;

    private void Start()
    {
        SetPrompt(false);
    }

    private void Update()
    {
        if (playerHead == null || isLoading)
        {
            SetPrompt(false);
            return;
        }

        Vector3 difference =
            playerHead.position - transform.position;

        float heightDifference = Mathf.Abs(difference.y);
        difference.y = 0f;

        bool nearby =
            difference.sqrMagnitude <=
            interactionDistance * interactionDistance &&
            heightDifference <= maximumHeightDifference;

        SetPrompt(nearby);

        bool pressedP =
        Keyboard.current != null &&
        Keyboard.current.pKey.wasPressedThisFrame;

        bool pressedNorth =
            Gamepad.current != null &&
            Gamepad.current.buttonNorth.wasPressedThisFrame;

        if (!nearby || (!pressedP && !pressedNorth))
            return;


        if (!Application.CanStreamedLevelBeLoaded(ScenePath))
        {
            Debug.LogError(
                "Chưa thêm DapNieuScene vào Scene List."
            );
            return;
        }

        isLoading = true;
        SetPrompt(false);

        SceneManager.LoadSceneAsync(
            ScenePath,
            LoadSceneMode.Single
        );
    }

    private void SetPrompt(bool visible)
    {
        if (prompt != null && prompt.activeSelf != visible)
            prompt.SetActive(visible);
    }

    private void OnDisable()
    {
        SetPrompt(false);
    }
}