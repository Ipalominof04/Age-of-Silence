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
    [SerializeField] private GameObject triggerCambioEscena;
    [Header("Shake")]
    [SerializeField] private CamaraTemblor cameraShake;
    [SerializeField] private float duracion = 3f;
    [SerializeField] private float fuerza = 0.5f;
    [Header("Cinematica")]
    [SerializeField] private Transform[] puntos;
    [SerializeField] private float velocidad = 3f;
    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;
    private Animator animator;
    private Transform playerTransform;
    private SoundEmitter emitter;
    private int puntoActual = 0;
    private bool activo = false;
    private void Awake()
    {
        emitter = GetComponent<SoundEmitter>();
        GameObject player = GameObject.FindWithTag("Player");
        animator = player.GetComponent<Animator>();
        playerTransform = player.transform;
        triggerCambioEscena.SetActive(false);
    }
    // CINEMATICA
    private void IniciarCinematica()
    {
        activo = true;
    }
    void Update()
    {
        if (!activo || puntoActual >= puntos.Length) return;
        Transform objetivo = puntos[puntoActual];
        Vector3 direccion = (objetivo.position - playerTransform.position).normalized;
        direccion.y = 0f;
        if (direccion != Vector3.zero)
            playerTransform.rotation = Quaternion.LookRotation(direccion);
        playerTransform.position = Vector3.MoveTowards(playerTransform.position, objetivo.position, velocidad * Time.deltaTime);
        animator.Play("run");
        if (Vector3.Distance(playerTransform.position, objetivo.position) < 0.1f)
        {
            puntoActual++;
            if (puntoActual >= puntos.Length)
            {
                animator.Play("idle");
                activo = false;
            }
        }
    }
    // PUZLE
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
            objetoMano.ClearObject();
            GameManager.Instance.ResolverPuzzle("GalonUsado");
            emitter.EmitSound(ESound.GASOLINA);
        }
        else if (objeto == "Cerillas")
        {
            meshCerilla.SetActive(true);
            objetoMano.ClearObject();
            emitter.EmitSound(ESound.CERILLAS);
            StartCoroutine(SonarDespuesDeCerillas());
            cameraShake.ShakeContinuo(duracion, fuerza);
            triggerCambioEscena.SetActive(true);
            IniciarCinematica();
        }
    }
    public void OnEnterRange()
    {
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            canvasworld.SetActive(false);
        }
    }
    private IEnumerator SonarDespuesDeCerillas()
    {
        var soundData = SoundManager.Instance.soundRegistry.GetSound(ESound.CERILLAS);
        float duracionClip = soundData.clip != null ? soundData.clip.length : 1f;
        yield return new WaitForSeconds(duracionClip);
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
            s.spatialBlend = 0f;
        emitter.EmitSound(ESound.FUEGO);

        yield return new WaitForSeconds(5f);
        print("exploto");
        emitter.EmitSound(ESound.EXPLOSION);

        var exploData = SoundManager.Instance.soundRegistry.GetSound(ESound.EXPLOSION);
        float duracionExplo = exploData.clip != null ? exploData.clip.length : 1f;
        yield return new WaitForSeconds(duracionExplo);

    //    this.gameObject.SetActive(false);
    }
}