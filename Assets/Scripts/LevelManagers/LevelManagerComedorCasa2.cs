using UnityEngine;

public class NeveraPuzle : MonoBehaviour
{
    public RuntimeDialogueGraph graphDialogue;
    public void SubirConfianza()
    {
        DialogueManager.Instance.StartDialogue(graphDialogue);
    }
}
