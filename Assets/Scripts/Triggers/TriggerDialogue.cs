using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    public RuntimeDialogueGraph graphDialogue;
    [SerializeField] public GameEvent onDialogueFinished;

    private void OnEnable()
    {
        onDialogueFinished.Subscribe(DeactivateGameObject);
    }

    private void OnDisable()
    {
        onDialogueFinished.Unsubscribe(DeactivateGameObject);
    }

    private void DeactivateGameObject()
    {
        gameObject.SetActive(false);
    }
}