using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    [Header("Boton continuar")]
    [SerializeField] private Button boton;

    [SerializeField] public SOMenuName menuName;
    [SerializeField] public GameObject menuOptionsUI;
    [SerializeField] public GameObject mainMenuUI;

    public void Play()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs borrados. Tiene 'playerVolume': " + PlayerPrefs.HasKey("playerVolume"));

        string ruta = GameManager.Instance.ruta;
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);

        GameManager.Instance.LimpiarSOEscenasTotal();

        GameManager.Instance.datos.NameScene = menuName.nameScene;
        GameManager.Instance.datos.PlayerHealth = 6f;
        GameManager.Instance.datos.puzzlesResueltos.Clear();
        GameManager.Instance.lastDoorID = menuName.spawnName;
        GameManager.Instance.datos.aidenTieneOso = false;
        GameManager.Instance.confianza = 0;
        GameManager.Instance.datos.confianza = 0;
        GameManager.Instance.IrAScene(menuName.nameScene);
        PlayerController.Instance?.ClearArma();
        GameManager.Instance.datos.objetoMano.ClearObject();
        AidenController.Instance.ResetAidenPlay();
        StartCoroutine(OcultarTrasFade());
        
    }
    public void Continue()
    {
        GameManager.Instance.CargarPartida();
        StartCoroutine(OcultarTrasFade());
    }
    public void Opciones()
    {
        menuOptionsUI.SetActive(true);
    }
    public void Exit()
    {
        //para cerrarlo en la build
        Application.Quit();
        Debug.Log("Cerrando el juego...");
    }

    private IEnumerator OcultarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(false);
    }

}
