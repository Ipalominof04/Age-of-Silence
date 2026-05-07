using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    [Header("Boton continuar")]
    [SerializeField] private Button boton;

    [SerializeField] public SOMenuName menuName;
    [SerializeField] public GameObject menuOptionsUI;

    private void Start()
    {
        string ruta = GameManager.Instance.ruta;
        boton.interactable = File.Exists(ruta);
    }
    public void Play()
    {
        string ruta = GameManager.Instance.ruta;
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);

        GameManager.Instance.datos.NameScene = menuName.nameScene;
        GameManager.Instance.datos.PlayerHealth = 6f;
        GameManager.Instance.datos.puzzlesResueltos.Clear();

        GameManager.Instance.IrAScene(menuName.nameScene);
        this.gameObject.SetActive(false);
    }
    public void Continue()
    {
        GameManager.Instance.CargarPartida();
    }
    public void Opciones()
    {
        this.gameObject.SetActive(false);
        menuOptionsUI.SetActive(true);
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Cerrando el juego...");
    }



}
