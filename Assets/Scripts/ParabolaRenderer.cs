using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaRenderer : MonoBehaviour
{
    [Header("Simulación")]
    [SerializeField] private int puntos = 30;
    [SerializeField] private float tiempoSimulacion = 2f;
    [SerializeField] private float anguloLanzamiento = 45f;


    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    public Vector3 CalcularVelocidadInicial(Vector3 direccion, float fuerza)
    {
        Vector3 direccionConAngulo = Quaternion.AngleAxis(-anguloLanzamiento, Vector3.Cross(direccion, Vector3.up)) * direccion;
        return direccionConAngulo * fuerza;
    }


    public void MostrarParabola(Vector3 origen, Vector3 direccion, float fuerza)
    {
        Vector3 velocidadInicial = CalcularVelocidadInicial(direccion, fuerza);
        lineRenderer.enabled = true;
        lineRenderer.positionCount = puntos;

        float paso = tiempoSimulacion / puntos;

        for (int i = 0; i < puntos; i++)
        {
            float t = paso * i;
            // Fórmula cinemática: pos = origen + v*t + 0.5*g*t²
            Vector3 pos = origen
                + velocidadInicial * t
                + 0.5f * Physics.gravity * t * t;

            lineRenderer.SetPosition(i, pos);
        }
    }

    public void OcultarParabola()
    {
        lineRenderer.enabled = false;
    }
}