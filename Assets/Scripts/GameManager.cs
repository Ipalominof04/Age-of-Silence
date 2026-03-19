using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Valor")]
    public int Valor = 0;
    [Header("Fade")]
    private Image fadeImage;        // la UI Image negra
    public float fadeDuration = 1f;
    [Header("Spawn")]
    public string lastDoorID = ""; // guarda la puerta de la que venimos

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeImage();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SumValor(int amount)
    {
        Valor += amount;
        Debug.Log($"Valor subió a: {Valor}");
    }

    public void RestValor(int amount)
    {
        Valor -= amount;
        Debug.Log($"Valor bajó a: {Valor}");
    }

    private void CreateFadeImage()
    {
        // Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        DontDestroyOnLoad(canvasGO);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Image negra
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f); // negro transparente

        // Cubre toda la pantalla
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeImage.gameObject.SetActive(false);
    }

    public void ChangeScene(string s)
    {
        StartCoroutine(FadeAndLoadScene(s));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return null; // espera un frame a que cargue la escena
        yield return StartCoroutine(FadeIn());
    }
    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = fadeImage.color;
        fadeImage.gameObject.SetActive(true);
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (t / fadeDuration));
            fadeImage.color = c;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);
    }
}
