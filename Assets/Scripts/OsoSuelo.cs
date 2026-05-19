using UnityEngine;

public class OsoSuelo : MonoBehaviour
{
    [SerializeField] GameEvent OntakeBear;
    private GameObject osoMano;



    private void Start()
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
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
        OntakeBear.Unsubscribe(CogerOso);
    }
    private void CogerOso()
    {
        this.gameObject.SetActive(false);
        osoMano.SetActive(true);
    }
}