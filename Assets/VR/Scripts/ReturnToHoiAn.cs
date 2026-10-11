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
        if (isLoading || Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
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