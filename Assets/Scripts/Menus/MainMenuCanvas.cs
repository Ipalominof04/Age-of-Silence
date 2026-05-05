using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    [Header("Boton continuar")]
    [SerializeField] private Button boton;

    [SerializeField] public SOMenuName menuName;

    private void Start()
    {
        string ruta = GameManager.Instance.ruta;
        boton.interactable = File.Exists(ruta);
    }
    public void Play()
    {
        GameManager.Instance.datos.NameScene = menuName.nameScene;
        GameManager.Instance.IrAScene("LoadManagers");
    }

    public void Continue()
    {
        GameManager.Instance.CargarPartida();
    }
}
