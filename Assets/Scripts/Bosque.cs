using UnityEngine;

public class Bosque : MonoBehaviour
{
    [SerializeField] public SoundEmitter soundEmitter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundEmitter.EmitSound(ESound.BOSQUE);

        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
        {
            if (s.clip != null && s.isPlaying)
                s.spatialBlend = 0f;
        }
    }
}
