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

    private void Start()
    {
        UpdateVignette(SceneManager.GetActiveScene().name);
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

        vignetteObject.SetActive(active);

        Debug.Log($"Viñeta {(active ? "ACTIVA" : "DESACTIVADA")} en {sceneName}");
    }
}