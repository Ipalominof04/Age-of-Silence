using UnityEngine;

public class Rio : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    void Start()
    {
        emitter.SetRange(0, 30);
        emitter.EmitSound(ESound.RIO); 
    }
}
