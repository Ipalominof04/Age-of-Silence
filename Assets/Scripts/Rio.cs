using UnityEngine;

public class Rio : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emitter.EmitSound(ESound.RIO); 
    }
}
