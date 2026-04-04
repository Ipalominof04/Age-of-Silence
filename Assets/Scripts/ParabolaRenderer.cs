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

    [Header("Lanzamiento")]


    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
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

        float alturaMaxima = Mathf.Max(altura, desplazamiento.y + 0.1f);
        float tiempoSubida = Mathf.Sqrt(2 * alturaMaxima / gravedad);
        float tiempoTotal = tiempoSubida + Mathf.Sqrt(2 * (alturaMaxima - desplazamiento.y) / gravedad);

        Vector3 velocidadHorizontal = desplazamientoHorizontal / tiempoTotal;
        float velocidadVertical = gravedad * tiempoSubida;

        return velocidadHorizontal + Vector3.up * velocidadVertical;
    }

    public void MostrarParabola(Vector3 origen, Vector3 destino)
    {
        Vector3 velocidadInicial = CalcularVelocidadInicial(origen, destino);

        lineRenderer.enabled = true;

        List<Vector3> posiciones = new List<Vector3>();
        Vector3 puntoAnterior = origen;
        float t = 0;

        while (t < 10f)
        {
            t += paso;
            Vector3 pos = origen
                + velocidadInicial * t
                + 0.5f * Physics.gravity * t * t;

            // Comprobar distancia horizontal ANTES de añadir el punto
            float distanciaHorizontal = Vector3.Distance(
                new Vector3(pos.x, 0, pos.z),
                new Vector3(origen.x, 0, origen.z));

            if (distanciaHorizontal >= distanciaMaxima)
                break; // simplemente para, no añade el punto

            Vector3 direccion = pos - puntoAnterior;
            float distSegmento = direccion.magnitude;

            if (distSegmento > 0.001f && capasColision != 0)
            {
                if (Physics.Raycast(puntoAnterior, direccion.normalized, out RaycastHit hit, distSegmento, capasColision))
                {
                    posiciones.Add(hit.point);
                    break;
                }
            }

            posiciones.Add(pos);
            puntoAnterior = pos;

            if (Vector3.Distance(new Vector3(pos.x, 0, pos.z),
                new Vector3(destino.x, 0, destino.z)) < 0.3f && pos.y <= destino.y + 0.5f)
                break;

            if (pos.y < origen.y - 50f)
                break;
        }

        lineRenderer.positionCount = posiciones.Count;
        lineRenderer.SetPositions(posiciones.ToArray());
    }

    public void OcultarParabola()
    {
        lineRenderer.enabled = false;
    }
}