using UnityEngine;

public class OsoSuelo : MonoBehaviour
{
    [SerializeField] GameEvent OntakeBear;// event q es dispara en recollir l'os
    private GameObject osoMano; // referencia de l'os quan esta a la ma de aiden

    private void Start()
    {
        // busca entre tots els objectes carregats a la escena (inclosos inactius)
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // Quan troba el troba, el guarda
            if (obj.name == "OsoAiden")
            {
                osoMano = obj;
                break;
            }
        }
    }

    private void OnEnable()
    {
        OntakeBear.Subscribe(CogerOso);
    }
    private void OnDisable()
    {
        // es desuscriu per evitar errors o duplicats
        OntakeBear.Unsubscribe(CogerOso);
    }
    private void CogerOso()
    {
        this.gameObject.SetActive(false); // es desactiva l'os del terra
        osoMano.SetActive(true); // activa l'os de a la ma de Aiden
        GameManager.Instance.datos.aidenTieneOso = true; // guarda a les dades q aiden te l'os
    }
}