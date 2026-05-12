using UnityEngine;

public class Rio2 : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    void Start()
    {
        emitter.SetRange(minDistance, maxDistance);
        emitter.EmitSound(ESound.RIO2);
    }
}
