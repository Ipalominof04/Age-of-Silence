// Fuego.cs
using UnityEngine;

public class Fuego : MonoBehaviour
{
    [SerializeField] public SoundEmitter emitter;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    void Start()
    {
        emitter.SetRange(minDistance, maxDistance);
        emitter.EmitSound(ESound.FUEGO);
    }
}