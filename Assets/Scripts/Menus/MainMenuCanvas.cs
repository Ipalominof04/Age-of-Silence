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
        GameManager.Instance.datos.NameScene = "SigiloSalida";
=======
        GameManager.Instance.datos.NameScene = "CabañaNPC";
>>>>>>> 09a8d25 (arreglo escena2 + NPC y dialogo)
        GameManager.Instance.IrAScene("LoadManagers");
    }

    public void Continue()
    {
        GameManager.Instance.CargarPartida();
    }
}
