using UnityEngine;
public class TriggerDialogue : MonoBehaviour
{
    public RuntimeDialogueGraph graphDialogue;
    [SerializeField] public GameEvent onDialogueFinished;
    // L'OnEnable se subscriu a l'event de fi de dialeg per desactivar l'objecte quan acabi
    private void OnEnable()
    {
        onDialogueFinished.Subscribe(DeactivateGameObject);
    }
    // L'OnDisable es dessubscriu de l'event per evitar memory leaks
    private void OnDisable()
    {
        onDialogueFinished.Unsubscribe(DeactivateGameObject);
    }
    // El DeactivateGameObject desactiva l'objecte un cop el dialeg ha finalitzat
    private void DeactivateGameObject()
    {
        gameObject.SetActive(false);
    }
}