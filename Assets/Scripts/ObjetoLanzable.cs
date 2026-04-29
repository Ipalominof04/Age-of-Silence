using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    public GameObject hand;

    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision;
    [SerializeField] private ParticleSystem particulas;

    private SphereCollider objetoCollider;
    private SoundEmitter emitter;
    private bool yaLanzado = false;
    private Vector3 velocidadPendiente;
    private bool lanzarEnFixedUpdate = false;
    private bool estaEnMano = false;
    private Vector3 escalaMundialOriginal; // se guarda UNA sola vez en Awake

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
        AplicarEscala();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    private void AplicarEscala()
    {
        Vector3 parentScale = hand.transform.lossyScale;
        transform.localScale = new Vector3(
            escalaMundialOriginal.x / parentScale.x,
            escalaMundialOriginal.y / parentScale.y,
            escalaMundialOriginal.z / parentScale.z
        );
    }

    private void LateUpdate()
    {
        if (!estaEnMano || hand == null) return;
        AplicarEscala();
    }

    public void Lanzar(Vector3 velocidadInicial, Collider colliderJugador)
    {
        estaEnMano = false;
        yaLanzado = true;
        transform.SetParent(null);

        // Restaurar escala original al soltar
        transform.localScale = escalaMundialOriginal;

        objetoCollider.enabled = false;

        int layerObjeto = gameObject.layer;
        int layerPlayer = LayerMask.NameToLayer("Player");
        int layerPlayerCollider = LayerMask.NameToLayer("PlayerCollider");
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayer, true);
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayerCollider, true);

        velocidadPendiente = velocidadInicial;
        lanzarEnFixedUpdate = true;
    }

    private void FixedUpdate()
    {
        if (!lanzarEnFixedUpdate) return;
        lanzarEnFixedUpdate = false;
        objetoCollider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadPendiente;
        }

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
                    //las particulas son un hijo de la botella
                    //esto desvincula las partículas de la botella para que no se destruyan con ella
                    particulas.transform.SetParent(null);
                    //pone las partículas en el punto exacto de impacto
                    particulas.transform.position = collision.contacts[0].point;
                    particulas.Play();
                    //y las destruye 2 segundos despues
                    Destroy(particulas.gameObject, 2f);
                }
                Destroy(gameObject);
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