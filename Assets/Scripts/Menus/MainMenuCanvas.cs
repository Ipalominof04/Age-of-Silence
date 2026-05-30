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

    // La funcio de play borra el fitxer de json si es que hi ha algun i reseteja les variables per poder fer una nova partida
    public void Play()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); 

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
    // La funcio del continue no fa res que que una corrutina que truca a carregarPartida y desactiva el menu
    public void Continue()
    {
        GameManager.Instance.CargarPartida();
        StartCoroutine(OcultarTrasFade());
    }
    // La funcio de opcions activa el menu de opcions
    public void Opciones()
    {
        menuOptionsUI.SetActive(true);
    }
    // aquesta funcio serveix per tancar el joc
    public void Exit()
    {
        //para cerrarlo en la build
        Application.Quit();
        Debug.Log("Cerrando el juego...");
    }
    // Aquesta funcio posa el menu en desactivat despres de x temps.
    private IEnumerator OcultarTrasFade()
    {
        yield return new WaitForSeconds(GameManager.Instance.fadeDuration);
        mainMenuUI.SetActive(false);
    }

}
