using UnityEngine;

public class Alarma : MonoBehaviour
{
    [SerializeField] public SoundEmitter soundEmitter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundEmitter.EmitSound(ESound.ALARMA);

        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
        {
            if (s.clip != null && s.isPlaying)
                s.spatialBlend = 0f;
        }

    }
}
