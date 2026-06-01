using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject menuPausaUI; // panel menu pausa
    [SerializeField] private GameObject menuOpionsUI; // panel d'opcions (referenciat pero gestionat apart)
    [SerializeField] private GameObject mainMenuUI;  // panel del menu principal, s'activa al tornar despres del fade
    private InputAction menuPausaAction; // accio del input system que escolta la tecla de pausa
    private bool isPaused = false; // evita obrir el menu si ja esta pausat
    private void Start()
    {
        // Obte l'accio "MenuPausa" del mapa UI i se subscriu a ella
        var uiMap = InputManager.Instance.UIMap;
        menuPausaAction = uiMap.FindAction("MenuPausa");
        menuPausaAction.performed += OnMenuPausa;
    }
    private void OnDestroy()
    {
        // dessubscriu l'event al destruir l'objecte per evitar memory leaks
        menuPausaAction.performed -= OnMenuPausa;
    }
    private void OnMenuPausa(InputAction.CallbackContext ctx)
    {
        if (isPaused) return; // no obre el menu si ja esta pausat
        // no es pot pausar al MainMenu ni als credits
        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Creditos") return;
        // no s'activa el menu si el canvas del chapter ui esta actiu
        if (ChapterUI.Instance != null && ChapterUI.Instance.IsVisible) return;
        isPaused = true;
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f; // congela el joc
        InputManager.Instance.PlayerMap.Disable(); // bloqueja l'input del player
    }
    // btn menu principal dins del menu de pausa
    public void MainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // reprен el temps abans de canviar d'escena
        InputManager.Instance.PlayerMap.Enable();
        menuPausaUI.SetActive(false);
        GameManager.Instance.IrAScene("MainMenu"); // llanca la transicio amb fade cap al MainMenu
        StartCoroutine(ActivarTrasFade()); // espera al fade per mostrar la UI del main menu
    }
    // btn reprendre dins del menu de pausa
    public void Return()
    {
        isPaused = false;
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f; // reprен el temps
        InputManager.Instance.PlayerMap.Enable(); // retorna el control al player
    }
    // Espera que el fade de transicio acabi abans d'activar la UI del main menu
    // Evita que el panel aparegui sobre la pantalla de joc abans que acabi l'animacio
    private IEnumerator ActivarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(true);
    }
}