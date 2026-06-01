using System.Collections;
using UnityEngine;

public class TriggerPuzleGasolinera : MonoBehaviour, IPuzle
{
    // Evita que el missatge informatiu es mostri mes d'una vegada.
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
        // Obtenim el component encarregat dels sons
        // i desactivem el trigger de canvi d'escena.
        emitter = GetComponent<SoundEmitter>();
        triggerCambioEscena.SetActive(false);
    }

    private void Start()
    {
        // Busquem el jugador a l'escena.
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            return;
        }

        animator = player.GetComponent<Animator>();
        playerTransform = player.transform;
    }

    // Inicia la cinematica i bloqueja els controls del jugador.
    private void IniciarCinematica()
    {
        activo = true;
        InputManager.Instance.PlayerMap.Disable();
    }

    void Update()
    {
        // Si la cinematica no esta activa o ja hem arribat a l'ultim punt,
        // no fem res.
        if (!activo || puntoActual >= puntos.Length) return;

        Transform objetivo = puntos[puntoActual];

        // Calculem la direccio cap al seguent punt.
        Vector3 direccion = (objetivo.position - playerTransform.position).normalized;
        direccion.y = 0f;

        // Fem que el jugador miri cap a la direccio de moviment.
        if (direccion != Vector3.zero)
            playerTransform.rotation = Quaternion.LookRotation(direccion);

        // Movem el jugador cap al punt actual.
        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position,
            objetivo.position,
            velocidad * Time.deltaTime
        );

        animator.Play("run");

        // Quan arriba al punt, passem al seguent.
        if (Vector3.Distance(playerTransform.position, objetivo.position) < 0.1f)
        {
            puntoActual++;

            // Si ja no queden punts, acabem la cinematica.
            if (puntoActual >= puntos.Length)
            {
                animator.Play("idle");
                activo = false;
                InputManager.Instance.PlayerMap.Enable();
            }
        }
    }

    // Gestiona la interaccio amb els objectes del puzle.
    public void Interact(ObjetoAgarrado objetoMano)
    {
        // Si el jugador no porta cap objecte, sortim.
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            return;
        }

        string objeto = objetoMano.nombreObjeto;

        // Cas en que el jugador utilitza el galo de gasolina.
        if (objeto == "Galon")
        {
            meshGalon.SetActive(true);
            objetoMano.ClearObject();

            GameManager.Instance.ResolverPuzzle("GalonUsado");

            emitter.EmitSound(ESound.GASOLINA);
        }
        // Cas en que el jugador utilitza les cerilles.
        else if (objeto == "Cerillas")
        {
            meshCerilla.SetActive(true);
            objetoMano.ClearObject();

            emitter.EmitSound(ESound.CERILLAS);

            // Iniciem la sequencia de foc i explosio.
            StartCoroutine(SonarDespuesDeCerillas());

            // Apliquem el tremolor de camera.
            cameraShake.ShakeContinuo(duracion, fuerza);

            // Activem el canvi d'escena.
            triggerCambioEscena.SetActive(true);

            // Comencem la cinematica.
            IniciarCinematica();
        }
    }

    public void OnEnterRange()
    {
        // Mostrem la informacio del puzle nomes la primera vegada.
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);

            // Amaguem el canvas informatiu.
            canvasworld.SetActive(false);
        }
    }

    // Reprodueix els sons del foc i de l'explosio amb els temps adequats.
    private IEnumerator SonarDespuesDeCerillas()
    {
        var soundData = SoundManager.Instance.soundRegistry.GetSound(ESound.CERILLAS);

        float duracionClip = soundData.clip != null ? soundData.clip.length : 1f;

        // Esperem que acabi el so de les cerilles.
        yield return new WaitForSeconds(duracionClip);

        // Fem que els seguents sons sonin de manera global.
        AudioSource[] sources = GetComponents<AudioSource>();

        foreach (var s in sources)
            s.spatialBlend = 0f;

        // Reproduim el so del foc.
        emitter.EmitSound(ESound.FUEGO);

        yield return new WaitForSeconds(5f);

        // Reproduim el so de l'explosio.
        emitter.EmitSound(ESound.EXPLOSION);

        var exploData = SoundManager.Instance.soundRegistry.GetSound(ESound.EXPLOSION);

        float duracionExplo = exploData.clip != null ? exploData.clip.length : 1f;

        // Esperem que finalitzi el so de l'explosio.
        yield return new WaitForSeconds(duracionExplo);
    }
}