using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI texto;
    private Material mat;// material duplicat per modificar el glow sense afectar altres textos
    public float velocidad = 5f;
    private float glowObjetivo;


    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
        // crea una copia del mat per evitar modificar el material compartit
        mat = new Material(texto.fontMaterial);
        texto.fontMaterial = mat; // assigna el mat duplicat al text
    }

    void Update()
    {
        // llegeix el valor actual del glow del material
        float glowActual = mat.GetFloat(ShaderUtilities.ID_GlowPower);
        // interpola suavement cap al valor objectiu
        float nuevoGlow = Mathf.Lerp(glowActual, glowObjetivo, Time.unscaledDeltaTime * velocidad);
        // aplica el now valor del glow al material
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