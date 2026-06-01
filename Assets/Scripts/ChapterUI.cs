using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChapterUI : MonoBehaviour
{
    public static ChapterUI Instance;
    [Header("Referencias")]
    [SerializeField] private GameObject canvas; // canvas que conte el text del capitol
    [SerializeField] private TextMeshProUGUI texto; // text on es mostra el capitol
    [Header("Fade")]
    [SerializeField] private float fadeTime = 1f; // duracio de cada fade in/out
    // perque altres scripts sapiguen si aquest canvas esta visible
    public bool IsVisible => canvas.activeSelf;
    // L'Awake inicialitza el singleton, fa que persisteixi entre escenes i amaga el canvas fins que calgui
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persisteix entre escenes
            canvas.SetActive(false); // comenca amagat fins que es necessiti
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // El MostrarCapitulo bloqueja l'input, mostra un unic text de capitol amb fade i el torna a amagar
    public IEnumerator MostrarCapitulo(string mensaje, float duracion)
    {
        InputManager.Instance.PlayerMap.Disable(); // bloqueja l'input perque el player no es pugui moure
        canvas.SetActive(true);
        yield return PlayStep(mensaje, duracion); // fade in, espera, fade out
        canvas.SetActive(false);
        InputManager.Instance.PlayerMap.Enable(); // retorna el control al player
    }
    // El PlaySequence mostra una sequencia de textos un darrere l'altre (intro de capitol amb diversos missatges)
    public IEnumerator PlaySequence(ChapterStep[] steps)
    {
        InputManager.Instance.PlayerMap.Disable();
        canvas.SetActive(true);
        // executa cada step en ordre, esperant que cada un completi el seu fade in/out
        foreach (var step in steps)
        {
            yield return PlayStep(step.texto, step.duracion);
        }
        canvas.SetActive(false);
        InputManager.Instance.PlayerMap.Enable();
    }
    // El PlayStep assigna el text i executa el fade in, l'espera i el fade out d'un step individual
    private IEnumerator PlayStep(string msg, float duracion)
    {
        texto.text = msg;
        yield return Fade(0f, 1f); // fade in
        yield return new WaitForSeconds(duracion); // el text roman visible aquests segons
        yield return Fade(1f, 0f); // fade out
    }
    // El Fade interpola l'alpha del text entre dos valors al llarg de fadeTime segons
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = texto.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeTime); // calcula l'alpha proporcional al temps transcorregut
            texto.color = c;
            yield return null;
        }
        // forca el valor final exacte per evitar imprecisions de floating point
        c.a = to;
        texto.color = c;
    }
    // El ChapterStep es un struct serialitzable que representa un text amb la seva duracio dins d'una sequencia
    // En ser serialitzable apareix editable a l'Inspector de Unity
    [System.Serializable]
    public struct ChapterStep
    {
        public string texto;   // missatge a mostrar en aquest step
        public float duracion;  // segons que roman visible abans del fade out
        public ChapterStep(string texto, float duracion)
        {
            this.texto = texto;
            this.duracion = duracion;
        }
    }
}