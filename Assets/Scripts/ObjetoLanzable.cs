using UnityEngine;

public class ObjetoLanzable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;

    [Header("Hand")]
    [SerializeField] public GameObject hand;

    [Header("Lanzamiento")]
    [SerializeField] public float fuerzaLanzamiento = 15f;

    private SphereCollider collider;


    private void Awake()
    {
        collider = GetComponent<SphereCollider>();

        if (canvasWorld != null)
            canvasWorld.SetActive(false);

    }

    // ---------- UI ----------
    public void MostrarUI()
    {
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
    }

    public void Lanzar(Vector3 direccion)
    {
        transform.SetParent(null);
        collider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(direccion * fuerzaLanzamiento, ForceMode.Impulse);
        }
    }
}