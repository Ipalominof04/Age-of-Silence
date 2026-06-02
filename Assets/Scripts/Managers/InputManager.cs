using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    [SerializeField] private InputActionAsset inputActionAsset;
    public InputActionAsset Actions { get; private set; }
    public InputActionMap PlayerMap { get; private set; }
    public InputActionMap UIMap { get; private set; }
    public InputActionMap GodModeMap { get; private set; }
    // L'Awake inicialitza el singleton, instancia l'InputActionAsset i habilita els mapes d'input del jugador, la UI i el GodMode
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Actions = Instantiate(inputActionAsset);
        PlayerMap = Actions.FindActionMap("Player");
        UIMap = Actions.FindActionMap("UI");
        GodModeMap = Actions.FindActionMap("GodMode");
        PlayerMap.Enable();
        UIMap.Enable();
        GodModeMap.Enable();
    }
}