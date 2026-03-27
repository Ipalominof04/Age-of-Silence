using UnityEngine;

public class OsoSuelo : MonoBehaviour
{
    [SerializeField] GameEvent OntakeBear;
    [SerializeField] GameObject osoMano;

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