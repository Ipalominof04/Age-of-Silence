using UnityEngine;

public class WaypointTriggerPuerta : MonoBehaviour
{
    [SerializeField] GameObject triggerPuerta;
    private SoundEmitter soundEmitter;

    private void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Aiden"))
        {
            soundEmitter.SetRange(1f, 50f);
            soundEmitter.EmitSound(ESound.PUERTA_PUZZLE);
            triggerPuerta.SetActive(true);

        }
    }
}
