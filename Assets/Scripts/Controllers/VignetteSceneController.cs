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
        //actualitza estat de la vinyeta segons l'escena carregada
        UpdateVignette(scene.name);
    }

    private void UpdateVignette(string sceneName)
    {
        // comprova si l'escena actual es dins de la llista de escenes amb vinyeta activa
        bool active = System.Array.Exists(
            scenesWithVignette,
            s => s == sceneName
        );
        // si existeix el sistema DamageVignette, el controla directament
        if (DamageVignette.Instance != null)
            DamageVignette.Instance.SetVignetteActive(active);
        else // si no, activa o desactiva objecte manualment
            vignetteObject.SetActive(active);
    }


}