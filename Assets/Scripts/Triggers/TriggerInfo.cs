using UnityEngine;

public class TriggerInfo : MonoBehaviour
{
    public RuntimeDialogueGraph graphInfo;

    //para que la info solo salga la primera vez que el player se acerca
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.StartInfo(graphInfo);
            gameObject.SetActive(false);
        }
    }
}