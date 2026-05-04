using System.Collections;
using UnityEngine;

public class TriggerPuzleGasolinera : MonoBehaviour, IPuzle
{

    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Meshes")]
    [SerializeField] private GameObject meshGalon;
    [SerializeField] private GameObject meshCerilla;
    [Header("Shake")]
    [SerializeField] private CamaraTemblor cameraShake;
    [SerializeField] private float duracion = 3f;
    [SerializeField] private float fuerza = 0.5f;

    private SoundEmitter emitter;
    [SerializeField] private CinematicaGasolinera cinematicaMovimiento;

    private void Awake()
    {
        emitter = GetComponent<SoundEmitter>();
    }

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            print("no tienes nada en la mano");
            return;
        }
        string objeto = objetoMano.nombreObjeto;

        if (objeto == "Galon")
        {
            meshGalon.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("GalonUsado");
            emitter.EmitSound(ESound.GASOLINA);

        }
        else if (objeto == "Cerillas")
        {
            meshCerilla.SetActive(true);
            objetoMano.UsarObjeto();
            emitter.EmitSound(ESound.CERILLAS);
            StartCoroutine(SonarDespuesDeCerillas());
            cameraShake.ShakeContinuo(duracion, fuerza);
           // cinematicaMovimiento.Iniciar();

        }
    }

    public void OnEnterRange()
    {
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
        }
    }

    private IEnumerator SonarDespuesDeCerillas()
    {
        var soundData = SoundManager.Instance.soundRegistry.GetSound(ESound.CERILLAS);
        float duracionClip = soundData.clip != null ? soundData.clip.length : 1f;

        yield return new WaitForSeconds(duracionClip);

        emitter.EmitSound(ESound.FUEGO);

        var fuegoData = SoundManager.Instance.soundRegistry.GetSound(ESound.FUEGO);
        float duracionFuego = fuegoData.clip != null ? fuegoData.clip.length : 1f;
        yield return new WaitForSeconds(duracionFuego);

        this.gameObject.SetActive(false);
    }
}
