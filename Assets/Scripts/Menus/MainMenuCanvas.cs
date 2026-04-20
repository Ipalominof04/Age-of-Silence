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
<<<<<<< HEAD
        GameManager.Instance.datos.NameScene = "PuenteLevadizo";
=======
        GameManager.Instance.datos.NameScene = "ExteriorGasolinera";
>>>>>>> 8103bc5 (uff luces basta)
        GameManager.Instance.IrAScene("LoadManagers");
    }

    public void Continue()
    {
        GameManager.Instance.CargarPartida();
    }
}
