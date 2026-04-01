using UnityEngine;

public class ObjetoLanzable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;

    [Header("Hand")]
    [SerializeField] public GameObject hand;

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

    public void lanzar()
    {

    }
}