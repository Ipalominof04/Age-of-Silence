using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;
    [Header("Hand")]
    [SerializeField] public GameObject hand;
    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision;

    private SphereCollider collider;
    private SoundEmitter emitter;
    private bool siendoLanzado = false;
    private bool yaLanzado = false;


    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
        emitter = GetComponent<SoundEmitter>();
        if (canvasWorld != null)
            canvasWorld.SetActive(false);
    }

    public void MostrarUI()
    {
        if (siendoLanzado) return;
        if (canvasWorld != null)
            canvasWorld.SetActive(true);
    }

    public void OcultarUI()
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(false);
    }
    public void Recoger()
    {
        collider.enabled = false;
        transform.SetParent(hand.transform);
        OcultarUI();
        transform.position = hand.transform.position;
        transform.rotation = hand.transform.rotation;
        transform.localScale = Vector3.one * 0.7f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true; // desactiva física mientras está en mano
    }

    public void Lanzar(Vector3 velocidadInicial, Collider colliderJugador)
    {
        siendoLanzado = true;
        yaLanzado = true;
        transform.SetParent(null);
        collider.enabled = true;

        if (canvasWorld != null)
            canvasWorld.SetActive(false);

        if (colliderJugador != null)
            Physics.IgnoreCollision(collider, colliderJugador, true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadInicial;
        }

        StartCoroutine(ReactivarColision(colliderJugador));
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
                emitter.EmitSound(ESound.ROCK);
            }
        }
    }
}