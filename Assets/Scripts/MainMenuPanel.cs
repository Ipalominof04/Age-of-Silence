using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button boton;
    private void OnEnable()
    {
        StartCoroutine(ActualizarBoton());
    }

    private IEnumerator ActualizarBoton()
    {
        yield return new WaitUntil(() => GameManager.Instance != null);
        string ruta = GameManager.Instance.ruta;
        boton.gameObject.SetActive(File.Exists(ruta));
    }
}