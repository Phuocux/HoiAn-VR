using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class ResetSimulatorOnEntry : MonoBehaviour
{
    [SerializeField] private GameObject simulatorPrefab;

    private IEnumerator Start()
    {
        if (simulatorPrefab == null)
        {
            Debug.LogError("Chưa gán Simulator Prefab.");
            yield break;
        }

        // Chờ hệ thống khởi tạo scene xong.
        yield return null;

        XRInteractionSimulator oldSimulator =
            XRInteractionSimulator.instance;

        if (oldSimulator != null)
        {
            oldSimulator.gameObject.SetActive(false);
            Destroy(oldSimulator.gameObject);

            // Chờ Simulator cũ được hủy hoàn toàn.
            yield return null;
        }

        GameObject newSimulator = Instantiate(simulatorPrefab);
        DontDestroyOnLoad(newSimulator);
    }
}