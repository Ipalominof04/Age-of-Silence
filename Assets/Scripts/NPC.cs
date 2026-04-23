using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] GameEvent misionComidaNPC;
    [SerializeField] GameEvent amenazaNPC;


    private void OnEnable()
    {
        amenazaNPC.Subscribe(AñadirPuzleAmenaza);
        misionComidaNPC.Subscribe(AñadirPuzleComida);
    }

    private void OnDisable()
    {
        amenazaNPC.Unsubscribe(AñadirPuzleAmenaza);
        misionComidaNPC.Unsubscribe(AñadirPuzleComida);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AñadirPuzleComida()
    {
        GameManager.Instance.datos.puzzlesResueltos.Add("MisionComidaNPC");
    }

    private void AñadirPuzleAmenaza()
    {
        GameManager.Instance.datos.puzzlesResueltos.Add("AmenazadoNPC");
    }
}
