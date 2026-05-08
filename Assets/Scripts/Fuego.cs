using UnityEngine;

public class Fuego : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    void Start()
    {
        emitter.EmitSound(ESound.FUEGO);
    }
}
