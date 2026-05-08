using UnityEngine;

public class Rio : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    void Start()
    {
        emitter.EmitSound(ESound.RIO); 
    }
}
