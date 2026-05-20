using UnityEngine;
using UnityEngine.SceneManagement;

public class VignetteSceneController : MonoBehaviour
{
    [Header("Objeto de la viñeta")]
    [SerializeField] private GameObject vignetteObject;

    [Header("Escenas donde se muestra")]
    [SerializeField] private string[] scenesWithVignette;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVignette(scene.name);
    }

    private void UpdateVignette(string sceneName)
    {
        bool active = System.Array.Exists(
            scenesWithVignette,
            s => s == sceneName
        );
        Debug.Log($"[VIÑETA] Instance null: {DamageVignette.Instance == null} | active: {active}");
        Debug.Log("[VIÑETACONTROLLER]" + sceneName + "estat: " +  active);
        if (DamageVignette.Instance != null)
            DamageVignette.Instance.SetVignetteActive(active);
        else
            vignetteObject.SetActive(active);

        Debug.Log($"Viñeta {(active ? "ACTIVA" : "DESACTIVADA")} en {sceneName}");
    }


}