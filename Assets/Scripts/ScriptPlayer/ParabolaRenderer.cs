using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaRenderer : MonoBehaviour
{
    // requereix de un line renderer per dibuixar la trajectoria
    [Header("Simulación")]
    [SerializeField] private float distanciaMaxima;
    [SerializeField] private float altura = 3f;
    //increment de temps per cada pas de la simulacio (precisio de trajectoria)
    [SerializeField] private float paso = 0.02f;
    [SerializeField] private LayerMask capasColision;
    //quantitat de fregament del moviment de l'objecte
    [SerializeField] private float dragObjeto = 0f;

    [Header("Indicador de caída")]
    [SerializeField] private int segmentosCirculo = 32;
    [SerializeField] private float radioCirculo = 0.4f;

    private LineRenderer lineRenderer; // dibuixa la trajectoria de la parabola
    private LineRenderer circuloRenderer;// dibuixa el cercle al punt de caiguda
    private GameObject circuloObj; // gameobject q conte el LineRenderer del cercle

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false; // es desactiva pq nomes es mostri quan es dibuxi la trajectoria

        circuloObj = new GameObject("IndicadorCaida"); // obj buit q dibuixara el cercle de caiguda
        circuloObj.transform.SetParent(transform);
        circuloRenderer = circuloObj.AddComponent<LineRenderer>();
        circuloRenderer.material = lineRenderer.material; // assigna el mateix mat q el de la parabola
        // defineix gruix de la linea del cercle
        circuloRenderer.startWidth = 0.05f;
        circuloRenderer.endWidth = 0.05f;
        // fa que el LineRenderer tanqui la forma connectant ultim punt amb el primer (cercle)
        circuloRenderer.loop = true;
        // indica que les coordenades són en espai mundial(no local)
        circuloRenderer.useWorldSpace = true;
        circuloRenderer.enabled = false;
    }

    // calcula el punt objectiu cap on apunta el ratoli dins un pla horitzontal
    public Vector3 ObtenerPuntoObjetivo(Vector3 origen, Camera camara)
    {
        // crea un ray desde el punt de la camara fins a la posicio del ratoli
        Ray ray = camara.ScreenPointToRay(Mouse.current.position.ReadValue());
        // pla horitzonatl a l'altura del origen
        Plane suelo = new Plane(Vector3.up, new Vector3(0, origen.y, 0));

        Vector3 puntoObjetivo;
        // calcula intersseccio del raig amb  el pla
        if (suelo.Raycast(ray, out float distancia))
            puntoObjetivo = ray.GetPoint(distancia);
        else
            puntoObjetivo = origen + Vector3.forward * distanciaMaxima;

        // limita la distancia max horitzontal
        Vector3 direccionHorizontal = puntoObjetivo - origen;
        direccionHorizontal.y = 0;
        if (direccionHorizontal.magnitude > distanciaMaxima)
            puntoObjetivo = origen + direccionHorizontal.normalized * distanciaMaxima;
        // mante la mateixa altura q l'origen
        puntoObjetivo.y = origen.y;
        return puntoObjetivo;
    }

    // calcula la velocitat inicial necessaria pq el projectil arribi al desti
    public Vector3 CalcularVelocidadInicial(Vector3 origen, Vector3 destino)
    {
        float gravedad = Mathf.Abs(Physics.gravity.y);
        Vector3 desplazamiento = destino - origen;
        Vector3 desplazamientoHorizontal = new Vector3(desplazamiento.x, 0, desplazamiento.z);
        float distanciaHorizontal = desplazamientoHorizontal.magnitude;

        // si esta massa a prop no hi ha moviment
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

    // dibuixa la parabola completa i detecta colisions
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
        // simulacio pas a pas del moviment del projectil
        while (t < 10f)
        {
            t += paso;

            posicion += velocidad * paso;
            velocidad += Physics.gravity * paso;
            velocidad *= Mathf.Clamp01(1f - dragObjeto * paso);

            // limit distancia horitzontal
            float distanciaHorizontal = Vector3.Distance(
                new Vector3(posicion.x, 0, posicion.z),
                new Vector3(origen.x, 0, origen.z));

            if (distanciaHorizontal >= distanciaMaxima)
                break;

            // colisio obstacles
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

            // evita simulacions infinites cap avall
            if (posicion.y < origen.y - 50f)
                break;
        }

        // assigna punts al LineRenderer
        lineRenderer.positionCount = posiciones.Count;
        lineRenderer.SetPositions(posiciones.ToArray());

        // detecta el punt real de contacte amb el terra
        if (Physics.Raycast(ultimoPunto + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 100f, capasColision))
            MostrarCirculo(hitSuelo.point);
        else
            MostrarCirculo(ultimoPunto);
    }
    // dibuixa el cercle al punt de caiguda
    private void MostrarCirculo(Vector3 centro)
    {
        circuloRenderer.enabled = true;
        circuloRenderer.positionCount = segmentosCirculo;

        Vector3 centroElevado = centro + Vector3.up * 0.05f;

        // genera punts en forma de cercle
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