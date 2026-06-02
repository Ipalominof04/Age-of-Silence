using UnityEngine;

public class WaypointTriggerPuerta : MonoBehaviour
{
    [SerializeField] GameObject triggerPuerta; // objecte q s'activara quan aiden entri al trigger
    private SoundEmitter soundEmitter; // referencia al sistema de so q emet els efectes de audio

    private void Awake()
    {
        // obte el component SoundEmitter del mateiz GameObject
        soundEmitter = GetComponent<SoundEmitter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Aiden"))
        {
            soundEmitter.SetRange(1f, 50f); // ajusta el rang de so
            soundEmitter.EmitSound(ESound.PUERTA_PUZZLE); // emet el so de la porta del puzle
            triggerPuerta.SetActive(true); //activa el trigger de la porta q permet al jugador canviar de escena

        }
    }
}
