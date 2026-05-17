using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Hace scroll vertical del texto de créditos (RectTransform sube).
/// Coloca este script en el GameObject que contiene el TMP_Text de créditos.
/// El RectTransform de ese objeto debe estar dentro de un panel con Mask o RectMask2D.
/// Al terminar llama a GameManager.ChangeScene("MainMenu").
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El TextMeshPro con el texto de los créditos")]
    [SerializeField] private RectTransform creditsRect;

    [Header("Velocidad")]
    [Tooltip("Píxeles por segundo a los que suben los créditos")]
    [SerializeField] private float scrollSpeed = 60f;

    [Tooltip("Posición Y inicial (parte baja de la pantalla). Déjalo a 0 para calcularlo automáticamente")]
    [SerializeField] private float startYOverride = 0f;

    private float endY;
    private float startY;
    private bool isScrolling = false;

    private void Start()
    {
        if (creditsRect == null)
        {
            Debug.LogError("[CreditsScroller] Asigna el RectTransform del TMP en el Inspector.");
            return;
        }

        StartCoroutine(InitAndScroll());
    }

    private IEnumerator InitAndScroll()
    {
        // Esperar un frame para que el layout se calcule correctamente
        yield return null;

        Canvas.ForceUpdateCanvases();

        float panelHalfHeight = GetComponentInParent<RectTransform>().rect.height * 0.5f;
        float textHeight = creditsRect.rect.height;

        startY = startYOverride != 0f ? startYOverride : -panelHalfHeight - textHeight * 0.5f;
        endY = panelHalfHeight + textHeight * 0.5f;

        creditsRect.anchoredPosition = new Vector2(creditsRect.anchoredPosition.x, startY);

        isScrolling = true;
    }

    private void Update()
    {
        if (!isScrolling || creditsRect == null) return;

        Vector2 pos = creditsRect.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        creditsRect.anchoredPosition = pos;

        if (pos.y >= endY)
        {
            isScrolling = false;
            GameManager.Instance.IrAScene("MainMenu");
        }
    }

    /// <summary>Cambia la velocidad en tiempo de ejecución.</summary>
    public void SetSpeed(float newSpeed) => scrollSpeed = newSpeed;
}