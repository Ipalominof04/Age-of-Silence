using UnityEngine;
using UnityEngine.UI;

public class Sobrecalentamiento : MonoBehaviour
{
    [SerializeField] Gradient gradient;

    [Header("Calentamiento")]
    [SerializeField] private Image barraCalor;
    [SerializeField] private float calorDisparo = 0.1f;
    private float calorActual;
    private bool sobrecalentada = false;

    [Header("Enfriamiento")]
    [SerializeField] private float tiempoEsperaEnfriado = 1f;
    [SerializeField] private float velocidadEnfriado = 0.5f;
    [SerializeField] private float tiempoEsperaSobrecalentada = 2f;

    private float tiempoUltimoDisparo;
    private bool enEsperaSobrecalentada = false; // Est� en el delay de 2s, no se enfr�a

    [Header("Eventos")]
    [SerializeField] GameEvent onShoot;
    [SerializeField] GameEvent onArmaSobrecalentada;
    [SerializeField] GameEvent onArmaEnfriada;


    void Update()
    {
        tiempoUltimoDisparo += Time.deltaTime;

        // Solo enfr�a si: pas� el tiempo de espera normal, hay calor, y NO est� en el delay de sobrecalentamiento
        if (!enEsperaSobrecalentada &&
            tiempoUltimoDisparo >= tiempoEsperaEnfriado &&
            calorActual > 0f)
        {
            calorActual -= velocidadEnfriado * Time.deltaTime;
            calorActual = Mathf.Clamp01(calorActual);
            ActualizarBarra();

            // Al bajar a 0.5 estando sobrecalentada, se desbloquea el disparo
            if (sobrecalentada && calorActual <= 0.5f)
            {
                sobrecalentada = false;
                onArmaEnfriada.Raise();
            }
        }
    }

    private void OnEnable()
    {
        onShoot.Subscribe(CalentarArma);
    }

    private void OnDisable()
    {
        onShoot.Unsubscribe(CalentarArma);
    }

    private void CalentarArma()
    {
        // Bloquear disparos si est� sobrecalentada
        if (sobrecalentada) return;

        calorActual += calorDisparo;
        calorActual = Mathf.Clamp01(calorActual);
        ActualizarBarra();

        if (calorActual >= 1f)
        {
            sobrecalentada = true;
            onArmaSobrecalentada.Raise();
            StartCoroutine(EsperarAntesDeEnfriar());
        }
    }

    private System.Collections.IEnumerator EsperarAntesDeEnfriar()
    {
        enEsperaSobrecalentada = true;
        yield return new WaitForSeconds(tiempoEsperaSobrecalentada);
        enEsperaSobrecalentada = false;
        // Tras el delay, el Update ya se encarga de enfriar normalmente
        tiempoUltimoDisparo = tiempoEsperaEnfriado; // Fuerza inicio de enfriamiento inmediato
    }

    private void ActualizarBarra()
    {
        barraCalor.fillAmount = calorActual;
        barraCalor.color = gradient.Evaluate(calorActual);
    }
}