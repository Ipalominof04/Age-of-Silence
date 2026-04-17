using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundEmitter))]
public class ObjetoLanzable : MonoBehaviour
{
    public GameObject hand;

    [Header("Colisión")]
    [SerializeField] private LayerMask capasColision;

    private SphereCollider objetoCollider;
    private SoundEmitter emitter;
    private bool yaLanzado = false;
    private Vector3 velocidadPendiente;
    private bool lanzarEnFixedUpdate = false;

    private void Awake()
    {
        objetoCollider = GetComponent<SphereCollider>();
        emitter = GetComponent<SoundEmitter>();
    }

    public void Recoger()
    {
        objetoCollider.enabled = false;
        transform.SetParent(hand.transform);
        transform.position = hand.transform.position;
        transform.rotation = hand.transform.rotation;

        if (this.tag == "Botella")
        {
            //compensar el scale del padre para que el scale MUNDIAL sea 0.7
            Vector3 parentScale = hand.transform.lossyScale;
            transform.localScale = new Vector3(
                0.7f / parentScale.x,
                0.7f / parentScale.y,
                0.7f / parentScale.z
                );
        }
        else if (this.tag == "Roca")
        {
            Vector3 parentScale = hand.transform.lossyScale;
            transform.localScale = new Vector3(
                0.1f / parentScale.x,
                0.1f / parentScale.y,
                0.1f / parentScale.z
            );
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public void Lanzar(Vector3 velocidadInicial, Collider colliderJugador)
    {
        yaLanzado = true;
        transform.SetParent(null);
        objetoCollider.enabled = false;

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