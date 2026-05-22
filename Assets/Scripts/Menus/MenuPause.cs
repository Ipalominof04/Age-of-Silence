using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject menuPausaUI;
    [SerializeField] private GameObject menuOpionsUI;
    [SerializeField] private GameObject mainMenuUI;
    private InputAction menuPausaAction;
    private bool isPaused = false;
    private void Start()
    {


        var uiMap = InputManager.Instance.UIMap;
        menuPausaAction = uiMap.FindAction("MenuPausa");
        menuPausaAction.performed += OnMenuPausa;
    }
    private void OnDestroy()
    {
        menuPausaAction.performed -= OnMenuPausa;
    }
    private void OnMenuPausa(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;
        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Creditos") return;

        isPaused = true;
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f;
        InputManager.Instance.PlayerMap.Disable();
    }
    public void MainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        InputManager.Instance.PlayerMap.Enable();
        menuPausaUI.SetActive(false);
        GameManager.Instance.IrAScene("MainMenu");
        StartCoroutine(ActivarTrasFade());
    }
    public void Return()
    {
        isPaused = false;
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        InputManager.Instance.PlayerMap.Enable();
    }

    private IEnumerator ActivarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(true);
    }
}