using UnityEngine;

public class WaypointTriggerPuerta : MonoBehaviour
{
    [SerializeField] GameObject triggerPuerta;


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Aiden"))
        {
            triggerPuerta.SetActive(true);
        }
    }
}
