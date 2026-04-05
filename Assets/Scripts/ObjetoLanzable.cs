using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] public GameObject hand;

    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision;

    private SphereCollider collider;
    private SoundEmitter emitter;
    private bool siendoLanzado = false;
    private bool yaLanzado = false;

    private Vector3 velocidadPendiente;
    private bool lanzarEnFixedUpdate = false;
    private Collider colliderJugadorPendiente;

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
        siendoLanzado = true;
        yaLanzado = true;
        transform.SetParent(null);
        collider.enabled = true;

        velocidadPendiente = velocidadInicial;
        colliderJugadorPendiente = colliderJugador;
        lanzarEnFixedUpdate = true;
    }

    private void FixedUpdate()
    {
        if (!lanzarEnFixedUpdate) return;
        lanzarEnFixedUpdate = false;

        if (colliderJugadorPendiente != null)
            Physics.IgnoreCollision(collider, colliderJugadorPendiente, true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadPendiente;
        }

        StartCoroutine(ReactivarColision(colliderJugadorPendiente));
    }

    private IEnumerator ReactivarColision(Collider colliderJugador)
    {
        yield return new WaitForSeconds(0.5f);
        if (colliderJugador != null && collider != null)
            Physics.IgnoreCollision(collider, colliderJugador, false);
        siendoLanzado = false;
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
                Destroy(gameObject);
            }

            if (this.gameObject.tag == "Roca")
            {
                if ((capasColision.value & (1 << collision.gameObject.layer)) == 0) return;
                emitter.EmitSound(ESound.ROCK);
            }
        }
    }
}