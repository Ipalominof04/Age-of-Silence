using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI texto;
    private Material mat;
    public float velocidad = 5f;
    private float glowObjetivo;


    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
        // Crea una instancia del material para no afectar otros textos
        mat = new Material(texto.fontMaterial);
        texto.fontMaterial = mat;
    }

    void Update()
    {
        float glowActual = mat.GetFloat(ShaderUtilities.ID_GlowPower);
        float nuevoGlow = Mathf.Lerp(glowActual, glowObjetivo, Time.deltaTime * velocidad);
        mat.SetFloat(ShaderUtilities.ID_GlowPower, nuevoGlow);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        glowObjetivo = 2f; // intensidad del brillo
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        glowObjetivo = 0f; // sin brillo
    }
}