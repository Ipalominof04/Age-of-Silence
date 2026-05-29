using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject menuPausaUI; // panel menu pausa
    [SerializeField] private GameObject menuOpionsUI; // panel de opciones (referenciado pero gestionado aparte)
    [SerializeField] private GameObject mainMenuUI;  // panel del menu principal, se activa al volver tras el fade

    private InputAction menuPausaAction; // accion del input system que escucha la tecla de pausa
    private bool isPaused = false; // evita abrir el menu si ya esta pausado
    private void Start()
    {
        // Obtiene la accion "MenuPausa" del mapa UI y se suscribe a ella
        var uiMap = InputManager.Instance.UIMap;
        menuPausaAction = uiMap.FindAction("MenuPausa");
        menuPausaAction.performed += OnMenuPausa;
    }
    private void OnDestroy()
    {
        // desuscribe el evento al destruir el objeto para evitar memory leaks
        menuPausaAction.performed -= OnMenuPausa;
    }
    private void OnMenuPausa(InputAction.CallbackContext ctx)
    {
        if (isPaused) return; // no abre el menu si ya esta pausado
        // no se puede pausar es MainMenu ni en creditos
        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Creditos") return;
        // no se activa el menu si el canvas de chapter ui esta activo
        if (ChapterUI.Instance != null && ChapterUI.Instance.IsVisible) return;

        isPaused = true;
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f; // congela el juego
        InputManager.Instance.PlayerMap.Disable();//bloquea input player
    }
    // btn menu principal dentro del menu de pausa
    public void MainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // reanuda tiempo antes de cambiar de escena
        InputManager.Instance.PlayerMap.Enable();
        menuPausaUI.SetActive(false);
        GameManager.Instance.IrAScene("MainMenu"); // lanza la transicion con fade hacia el MainMenu
        StartCoroutine(ActivarTrasFade());// espera al fade para mostrar el UI del main menu
    }
    // btn reanudar dentro del menu de pausa
    public void Return()
    {
        isPaused = false;
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f; // reanuda el tiempo
        InputManager.Instance.PlayerMap.Enable(); // devuelve control al player
    }

    // Espera a que el fade de transicion termine antes de activar el UI del main menu
    // Evita que el panel aparezca sobre la pantalla de juego antes de que acabe la animacion
    private IEnumerator ActivarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(true);
    }
}