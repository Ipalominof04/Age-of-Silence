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
    private bool enEsperaSobrecalentada = false;

    [Header("Enfriamiento")]
    [SerializeField] private float tiempoEsperaEnfriado = 1f;
    [SerializeField] private float velocidadEnfriado = 0.5f;
    [SerializeField] private float tiempoEsperaSobrecalentada = 2f;
    private float tiempoUltimoDisparo;
   
    [Header("Eventos")]
    [SerializeField] GameEvent onShoot;
    [SerializeField] GameEvent onArmaSobrecalentada;
    [SerializeField] GameEvent onArmaEnfriada;


    void Update()
    {
        //acumula el temps sense disparar
        tiempoUltimoDisparo += Time.deltaTime;

        //refreda l'arma si: no estem en enEsperaSobrecalentada (s'ha arribat a sobreescalfat l'arma)
        //refreda l'arma si: ha passat el temps de tiempoUltimoDisparo (si encara hi ha calor acumulada)
        if (!enEsperaSobrecalentada &&
            tiempoUltimoDisparo >= tiempoEsperaEnfriado &&
            calorActual > 0f)
        {
            calorActual -= velocidadEnfriado * Time.deltaTime;
            //mante el valor entre 0 i 1
            calorActual = Mathf.Clamp01(calorActual);
            ActualizarBarra();

            //quan baixa a 0.5 (la meitat de la barra, 50%) despres d'haber-se sobreescalfat l'arma, es desbloqueja el dispar
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

    //s'executa cada vegada que es dispara, augmenta el calor i comprova si s'ha sobreescalfat amb l'ultim tret
    private void CalentarArma()
    {
        //bloqueja el dispar si s'ha sobreescalfat
        if (sobrecalentada) return;

        calorActual += calorDisparo;
        calorActual = Mathf.Clamp01(calorActual);
        ActualizarBarra();

        //si la barra arriba a 1 (la barra completa, 100%), l'arma entra en estat de sobreescalfament
        if (calorActual >= 1f)
        {
            sobrecalentada = true;
            onArmaSobrecalentada.Raise();
            StartCoroutine(EsperarAntesDeEnfriar());
        }
    }

    //corrutina: bloqueja el refredament durant un temps adicional despres d'haber sobreescalfat l'arma com a penalitzacio (tiempoEsperaSobrecalentada)
    private System.Collections.IEnumerator EsperarAntesDeEnfriar()
    {
        //en espera de que es refredi
        enEsperaSobrecalentada = true;
        yield return new WaitForSeconds(tiempoEsperaSobrecalentada);
        //ha acabat la corrutina i s'ha refredat l'arma
        enEsperaSobrecalentada = false;
        //forca l'inici del refredament, l'update s'encarrega de que funcioni normalment despres de la corrutina
        tiempoUltimoDisparo = tiempoEsperaEnfriado; 
    }

    //actualitza la barra del canvas on es mostra el sobreescalfament per informar al jugador de l'estat de l'arma
    private void ActualizarBarra()
    {
        barraCalor.fillAmount = calorActual;
        barraCalor.color = gradient.Evaluate(calorActual);
    }
}