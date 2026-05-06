using UnityEngine;
using UnityEngine.InputSystem;
public class MenuPause : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject menuPausaUI;
    [SerializeField] private GameObject menuOpionsUI;
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
        GameManager.Instance.ChangeScene("MainMenu");
    }
    public void Return()
    {
        isPaused = false;
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        playerMap.Enable();
    }
}