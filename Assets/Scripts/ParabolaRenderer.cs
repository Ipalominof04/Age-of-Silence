using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaRenderer : MonoBehaviour
{
    [Header("Simulación")]
    [SerializeField] private float distanciaMaxima;
    [SerializeField] private float altura = 3f;
    [SerializeField] private float paso = 0.02f;
    [SerializeField] private LayerMask capasColision;
    [SerializeField] private float dragObjeto = 0f;

    [Header("Indicador de caída")]
    [SerializeField] private int segmentosCirculo = 32;
    [SerializeField] private float radioCirculo = 0.4f;

    private LineRenderer lineRenderer;
    private LineRenderer circuloRenderer;
    private GameObject circuloObj;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        circuloObj = new GameObject("IndicadorCaida");
        circuloObj.transform.SetParent(transform);
        circuloRenderer = circuloObj.AddComponent<LineRenderer>();
        circuloRenderer.material = lineRenderer.material;
        circuloRenderer.startWidth = 0.05f;
        circuloRenderer.endWidth = 0.05f;
        circuloRenderer.loop = true;
        circuloRenderer.useWorldSpace = true;
        circuloRenderer.enabled = false;
    }

    public Vector3 ObtenerPuntoObjetivo(Vector3 origen, Camera camara)
    {
        Ray ray = camara.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane suelo = new Plane(Vector3.up, new Vector3(0, origen.y, 0));

        Vector3 puntoObjetivo;
        if (suelo.Raycast(ray, out float distancia))
            puntoObjetivo = ray.GetPoint(distancia);
        else
            puntoObjetivo = origen + Vector3.forward * distanciaMaxima;

        Vector3 direccionHorizontal = puntoObjetivo - origen;
        direccionHorizontal.y = 0;
        if (direccionHorizontal.magnitude > distanciaMaxima)
            puntoObjetivo = origen + direccionHorizontal.normalized * distanciaMaxima;

        puntoObjetivo.y = origen.y;
        return puntoObjetivo;
    }

    public Vector3 CalcularVelocidadInicial(Vector3 origen, Vector3 destino)
    {
        float gravedad = Mathf.Abs(Physics.gravity.y);
        Vector3 desplazamiento = destino - origen;
        Vector3 desplazamientoHorizontal = new Vector3(desplazamiento.x, 0, desplazamiento.z);
        float distanciaHorizontal = desplazamientoHorizontal.magnitude;

        if (distanciaHorizontal < 0.01f)
            return Vector3.zero;

        float alturaMaxima = Mathf.Max(altura, desplazamiento.y + altura);
        float tiempoSubida = Mathf.Sqrt(2f * alturaMaxima / gravedad);
        float tiempoBajada = Mathf.Sqrt(2f * (alturaMaxima - desplazamiento.y) / gravedad);
        float tiempoTotal = tiempoSubida + tiempoBajada;

        float compensacionDrag = 1f + (dragObjeto * tiempoTotal * 0.5f);
        Vector3 velocidadHorizontal = (desplazamientoHorizontal / tiempoTotal) * compensacionDrag;
        float velocidadVertical = gravedad * tiempoSubida;

        return velocidadHorizontal + Vector3.up * velocidadVertical;
    }

    public void MostrarParabola(Vector3 origen, Vector3 destino)
    {
        Vector3 velocidadInicial = CalcularVelocidadInicial(origen, destino);

        lineRenderer.enabled = true;

        List<Vector3> posiciones = new List<Vector3>();
        Vector3 puntoAnterior = origen;
        Vector3 ultimoPunto = origen;

        Vector3 velocidad = velocidadInicial;
        Vector3 posicion = origen;
        float t = 0;

        while (t < 10f)
        {
            t += paso;

            posicion += velocidad * paso;
            velocidad += Physics.gravity * paso;
            velocidad *= Mathf.Clamp01(1f - dragObjeto * paso);

            // Límite de distancia horizontal
            float distanciaHorizontal = Vector3.Distance(
                new Vector3(posicion.x, 0, posicion.z),
                new Vector3(origen.x, 0, origen.z));

            if (distanciaHorizontal >= distanciaMaxima)
                break;

            // Colisión con obstáculos
            Vector3 direccion = posicion - puntoAnterior;
            float distSegmento = direccion.magnitude;

            if (distSegmento > 0.001f && capasColision != 0)
            {
                if (Physics.Raycast(puntoAnterior, direccion.normalized, out RaycastHit hit, distSegmento, capasColision))
                {
                    posiciones.Add(hit.point);
                    ultimoPunto = hit.point;
                    break;
                }
            }

            posiciones.Add(posicion);
            ultimoPunto = posicion;
            puntoAnterior = posicion;

            if (posicion.y < origen.y - 50f)
                break;
        }

        lineRenderer.positionCount = posiciones.Count;
        lineRenderer.SetPositions(posiciones.ToArray());

        // Raycast hacia abajo desde el último punto para encontrar el suelo real
        if (Physics.Raycast(ultimoPunto + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 100f, capasColision))
            MostrarCirculo(hitSuelo.point);
        else
            MostrarCirculo(ultimoPunto);
    }

    private void MostrarCirculo(Vector3 centro)
    {
        circuloRenderer.enabled = true;
        circuloRenderer.positionCount = segmentosCirculo;

        Vector3 centroElevado = centro + Vector3.up * 0.05f;

        for (int i = 0; i < segmentosCirculo; i++)
        {
            float angulo = 2f * Mathf.PI * i / segmentosCirculo;
            float x = Mathf.Cos(angulo) * radioCirculo;
            float z = Mathf.Sin(angulo) * radioCirculo;
            circuloRenderer.SetPosition(i, centroElevado + new Vector3(x, 0, z));
        }
    }

    public void OcultarParabola()
    {
        lineRenderer.enabled = false;
        circuloRenderer.enabled = false;
    }
}