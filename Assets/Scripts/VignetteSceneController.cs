using UnityEngine;
using UnityEngine.SceneManagement;

public class VignetteSceneController : MonoBehaviour
{
    [Header("Objeto de la vineta")]
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
        if (DamageVignette.Instance != null)
            DamageVignette.Instance.SetVignetteActive(active);
        else
            vignetteObject.SetActive(active);
    }


}