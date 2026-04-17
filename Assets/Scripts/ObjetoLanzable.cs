using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    public GameObject hand;

    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision;

    private SphereCollider collider;
    private SoundEmitter emitter;
    private bool yaLanzado = false;
    private Vector3 velocidadPendiente;
    private bool lanzarEnFixedUpdate = false;

    // 👇 NUEVO: guardamos colliders del jugador
    private Collider[] collidersJugadorIgnorados;

    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
        emitter = GetComponent<SoundEmitter>();
    }

    public void Recoger()
    {
        collider.enabled = false;
        transform.SetParent(hand.transform);
        transform.position = hand.transform.position;
        transform.rotation = hand.transform.rotation;

        if (this.tag == "Botella")
            transform.localScale = Vector3.one * 0.7f;
        else if (this.tag == "Roca")
            transform.localScale = Vector3.one * 0.15f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public void Lanzar(Vector3 velocidadInicial, Collider colliderJugador)
    {
        yaLanzado = true;
        transform.SetParent(null);
        collider.enabled = false;

        int layerObjeto = gameObject.layer;
        int layerPlayer = LayerMask.NameToLayer("Player");
        int layerPlayerCollider = LayerMask.NameToLayer("PlayerCollider");

        // 👇 Ignorar ambas layers inmediatamente
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayer, true);
        Physics.IgnoreLayerCollision(layerObjeto, layerPlayerCollider, true);

        velocidadPendiente = velocidadInicial;
        lanzarEnFixedUpdate = true;
    }

    private void FixedUpdate()
    {
        if (!lanzarEnFixedUpdate) return;
        lanzarEnFixedUpdate = false;

        collider.enabled = true;

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
        Debug.Log($"[{this.gameObject.name}] Colisión con: '{collision.gameObject.name}' " +
                  $"| Tag: '{collision.gameObject.tag}' " +
                  $"| Layer: '{LayerMask.LayerToName(collision.gameObject.layer)}'");

        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            if (this.gameObject.tag == "Botella")
            {
                if (!yaLanzado) return;
                if ((capasColision.value & (1 << collision.gameObject.layer)) == 0) return;
                emitter.EmitSound(ESound.BOTTLE);
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