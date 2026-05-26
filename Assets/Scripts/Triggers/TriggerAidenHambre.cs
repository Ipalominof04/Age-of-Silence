using UnityEngine;

public class TriggerAidenHambre : MonoBehaviour
{
    public RuntimeDialogueGraph graphDialogue;
    [SerializeField] public GameEvent onDialogueFinished;
    [SerializeField] public GameEvent onAidenHambre;

    private void OnEnable()
    {
        onAidenHambre.Subscribe(AddPuzle);
        onDialogueFinished.Subscribe(DeactivateGameObject);
    }

    private void OnDisable()
    {
        onAidenHambre.Unsubscribe(AddPuzle);
        onDialogueFinished.Unsubscribe(DeactivateGameObject);
    }

    private void DeactivateGameObject()
    {

        gameObject.SetActive(false);
    }

    private void AddPuzle()
    {
        GameManager.Instance.ResolverPuzzle("AidenNevera");
    }
}
