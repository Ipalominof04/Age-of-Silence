using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUI : MonoBehaviour
{
    public static ChapterUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject canvas; //canvas q contiene el texto del cap
    [SerializeField] private TextMeshProUGUI texto; // texto donde se muestra el cap

    [Header("Fade")]
    [SerializeField] private float fadeTime = 1f; //duracion de cada fade in/out

    //para que otros scripts sepan si este canvas esta visible
    public bool IsVisible => canvas.activeSelf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
            canvas.SetActive(false); // empieza oculto hasta q se necesite
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // muestra un unico texto del cap, espera y lo oculta
    public IEnumerator MostrarCapitulo(string mensaje, float duracion)
    {
        InputManager.Instance.PlayerMap.Disable(); // bloquea input para q el player no pueda moverse
        canvas.SetActive(true);
        yield return PlayStep(mensaje, duracion); // fade in, espera, fade out
        canvas.SetActive(false);
        InputManager.Instance.PlayerMap.Enable(); // devuelve control al player
    }

    // muestra una secuencia de textos uno tras otro (intro de capitulo con varios mensajes)
    public IEnumerator PlaySequence(ChapterStep[] steps)
    {
        InputManager.Instance.PlayerMap.Disable();
        canvas.SetActive(true);

        // ejecuta cada step en orden, esperando a que cada uno complete su fade in/out
        foreach (var step in steps)
        {
            yield return PlayStep(step.texto, step.duracion);
        }

        canvas.SetActive(false);
        InputManager.Instance.PlayerMap.Enable();
    }

    // logica interna de un step individual: asigna el texto y ejecuta fade in, espera, fade out
    private IEnumerator PlayStep(string msg, float duracion)
    {
        texto.text = msg;

        yield return Fade(0f, 1f); // fade in
        yield return new WaitForSeconds(duracion); // el texto permanece visible esos segundos
        yield return Fade(1f, 0f); // fade out
    }

    //Interpola el alpha del texto entre dos valores a lo largo de fadeTime segundos
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = texto.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeTime); // calcula el alpha proporcional al tiempo transcurrido
            texto.color = c;
            yield return null;
        }
        // fuerza el valor final exacto para evitar imprecisiones de floating point
        c.a = to;
        texto.color = c;
    }

    // Struct serializable que representa un texto con su duracion dentro de una secuencia
    // Al ser serializable aparece editable en el Inspector de Unity
    [System.Serializable]
    public struct ChapterStep
    {
        public string texto;   // mensaje a mostrar en este step
        public float duracion;  // segundos que permanece visible antes del fade out

        public ChapterStep(string texto, float duracion)
        {
            this.texto = texto;
            this.duracion = duracion;
        }
    }
}
