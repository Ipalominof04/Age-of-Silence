using UnityEngine;

public class ScriptNevera : MonoBehaviour
{
    [SerializeField] private GameEvent AidenHambre;
    [SerializeField] private GameObject nevera;

    private void OnEnable()
    {
        AidenHambre?.Subscribe(ActivarNevera);
    }

    private void OnDisable()
    {
        AidenHambre?.Unsubscribe(ActivarNevera);
    }

    private void ActivarNevera()
    {
        nevera.SetActive(true);
    }
}
