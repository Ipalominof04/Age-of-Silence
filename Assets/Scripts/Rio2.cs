using UnityEngine;

public class Rio2 : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    void Start()
    {
        emitter.EmitSound(ESound.RIO2);
    }
}
