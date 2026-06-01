using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    public GameObject hand; // referencia a la ma del player on agafa l'objecte

    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision; // capes obj pot colisionar
    [SerializeField] private ParticleSystem particulas; // particules es reprodueixen en impacte

    private SphereCollider objetoCollider;
    private SoundEmitter emitter;
    private bool yaLanzado = false; // evitar q objecte es processi multiples vegades despres de ser llancat
    private Vector3 velocidadPendiente; // velocitat guardada per aplicar en FixedUpdate
    private bool lanzarEnFixedUpdate = false; // controla quan aplicar el llancament fisic
    private bool estaEnMano = false; // indica si objecte esta agafat
    private Vector3 escalaMundialOriginal; // es guarda UNA vegada al awake la escala original del objecte

    private void Awake()
    {
        objetoCollider = GetComponent<SphereCollider>();
        emitter = GetComponent<SoundEmitter>();
        escalaMundialOriginal = transform.lossyScale;
        Debug.Log("escalaMundialOriginal en Awake: " + escalaMundialOriginal);
    }

    public void Recoger()
    {
        objetoCollider.enabled = false;

        transform.SetParent(hand.transform);
        transform.position = hand.transform.position;
        transform.rotation = hand.transform.rotation;

        estaEnMano = true;
        // ajusta escala perque no es deformi amb el parent
        AplicarEscala();

        // desactiva fisica mentre esta agafat
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    // ajusta escala tenit en compte escala del parent
    private void AplicarEscala()
    {
        Vector3 parentScale = hand.transform.lossyScale;
        transform.localScale = new Vector3(
            escalaMundialOriginal.x / parentScale.x,
            escalaMundialOriginal.y / parentScale.y,
            escalaMundialOriginal.z / parentScale.z
        );
    }

    // mante escala correcta mentre esta a la ma
    private void LateUpdate()
    {
        if (!estaEnMano || hand == null) return;
        AplicarEscala();
    }
    
    public void Lanzar(Vector3 velocidadInicial, Collider colliderJugador)
    {
        estaEnMano = false;
        yaLanzado = true;
        // desparenta objecte de la ma
        transform.SetParent(null);

        // el teu a escena actual per evitar problemes de jerarquia
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
            gameObject,
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        // Restaura escala original al soltar
        transform.localScale = escalaMundialOriginal;

        // desactiva collider temporalment per evitar problemes al moment de llancament
        objetoCollider.enabled = false;

        //
        int layerObjeto = gameObject.layer;
        int layerPlayer = LayerMask.NameToLayer("Player");
        int layerPlayerCollider = LayerMask.NameToLayer("PlayerCollider");
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayer, true);
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayerCollider, true);

        // guarda velocitat 
        velocidadPendiente = velocidadInicial;
        lanzarEnFixedUpdate = true;
    }

    //aplica fisica llancament al moment correcte
    private void FixedUpdate()
    {
        if (!lanzarEnFixedUpdate) return;
        lanzarEnFixedUpdate = false;
        objetoCollider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadPendiente; // aplica vel inicial
        }
        // reactiva colisions amb el jugador despres d'un temps
        StartCoroutine(ReactivarColision());
    }

    private IEnumerator ReactivarColision()
    {
        yield return new WaitForSeconds(0.5f);

        int layerObjeto = gameObject.layer;
        int layerPlayer = LayerMask.NameToLayer("Player");
        int layerPlayerCollider = LayerMask.NameToLayer("PlayerCollider");
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayer, false);
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayerCollider, false);
    }
    // deixa caure objecte sense forca
    public void Soltar()
    {
        estaEnMano = false;
        transform.SetParent(null);
        transform.localScale = escalaMundialOriginal;

        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        objetoCollider.enabled = true;
    }

    // detecta colisions amb el terra i executa efectes de particules i so
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            if (this.gameObject.tag == "Botella")
            {
                if (!yaLanzado) return;
                if ((capasColision.value & (1 << collision.gameObject.layer)) == 0) return;

                emitter.EmitSound(ESound.BOTTLE);

                if (particulas != null)
                {
                    particulas.transform.SetParent(null);
                    particulas.transform.position = collision.contacts[0].point;
                    particulas.Play();
                    Destroy(particulas.gameObject, 2f);
                }

                // Ocultar ampolla visualment pero mante el so
                GetComponent<MeshRenderer>().enabled = false;
                objetoCollider.enabled = false;
                Destroy(gameObject, 0.5f); // es destrueix despres del so
            }
            if (this.gameObject.tag == "Roca")
            {
                if (!yaLanzado) return;
                if ((capasColision.value & (1 << collision.gameObject.layer)) == 0) return;
                emitter.EmitSound(ESound.ROCK);
            }
        }
    }
}