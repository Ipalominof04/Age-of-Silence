using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    [Header("Boton continuar")]
    [SerializeField] private Button boton;

    private void Start()
    {
        string ruta = GameManager.Instance.ruta;
        boton.interactable = File.Exists(ruta);
    }
    public void Play()
    {
        GameManager.Instance.datos.NameScene = "SalidaFabrica";
        GameManager.Instance.IrAScene("LoadManagers");
    }

    public void Continue()
    {
        GameManager.Instance.CargarPartida();
    }
}
