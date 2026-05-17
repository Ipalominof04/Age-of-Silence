using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUI : MonoBehaviour
{
    public static ChapterUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvas.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // un texto del cap
    public IEnumerator MostrarCapitulo(string mensaje, float duracion)
    {
        canvas.SetActive(true);
        yield return PlayStep(mensaje, duracion);
        canvas.SetActive(false);
    }

    // secuencia de textos
    public IEnumerator PlaySequence(ChapterStep[] steps)
    {
        canvas.SetActive(true);

        //recorre todos los textos
        foreach (var step in steps)
        {
            yield return PlayStep(step.texto, step.duracion);
        }

        canvas.SetActive(false);
    }

    private IEnumerator PlayStep(string msg, float duracion)
    {
        texto.text = msg;

        yield return Fade(0f, 1f); // fade in
        yield return new WaitForSeconds(duracion);
        yield return Fade(1f, 0f); // fade out
    }

    // FADE DEL TEXTO
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = texto.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeTime);
            texto.color = c;
            yield return null;
        }

        c.a = to;
        texto.color = c;
    }

    // STRUCT PARA SECUENCIAS: guarda cada texto y duracion de cada step
    [System.Serializable]
    public struct ChapterStep
    {
        public string texto;
        public float duracion;

        public ChapterStep(string texto, float duracion)
        {
            this.texto = texto;
            this.duracion = duracion;
        }
    }
}
