using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class MenuPause : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject menuPausaUI;
    [SerializeField] private GameObject menuOpionsUI;
    [SerializeField] private GameObject mainMenuUI;
    private InputActionMap playerMap;
    private InputAction menuPausaAction;
    private bool isPaused = false;
    private void Start()
    {
        var input = Instantiate(inputActions);
        var uiMap = input.FindActionMap("UI");
        uiMap.Enable();
        playerMap = input.FindActionMap("Player");
        playerMap.Enable();
        menuPausaAction = uiMap.FindAction("MenuPausa");
        menuPausaAction.Enable();
        menuPausaAction.performed += OnMenuPausa;
    }
    private void OnDestroy()
    {
        menuPausaAction.performed -= OnMenuPausa;
        menuPausaAction.Disable();
    }
    private void OnMenuPausa(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;
        isPaused = true;
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f;
        playerMap.Disable();
    }
    public void MainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        playerMap.Enable();
        menuPausaUI.SetActive(false);
        GameManager.Instance.IrAScene("MainMenu");
        StartCoroutine(ActivarTrasFade());
    }
    public void Return()
    {
        isPaused = false;
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        playerMap.Enable();
    }

    private IEnumerator ActivarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(true);
    }
}