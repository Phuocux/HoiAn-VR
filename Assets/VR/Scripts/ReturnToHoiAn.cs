using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToHoiAn : MonoBehaviour
{
    private const string MapScenePath =
        "Assets/Scenes/HoiAnMap/SampleScene.unity";

    private bool isLoading;


    private void Update()
    {
        if (isLoading)
            return;

        bool escapePressed =
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame;

        bool startPressed =
            Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame;

        if (!escapePressed && !startPressed)
            return;

        if (!Application.CanStreamedLevelBeLoaded(MapScenePath))
        {
            Debug.LogError(
                "Chưa thêm SampleScene của HoiAnMap vào Scene List."
            );
            return;
        }

        isLoading = true;

        SceneManager.LoadSceneAsync(
            MapScenePath,
            LoadSceneMode.Single
        );
    }

}