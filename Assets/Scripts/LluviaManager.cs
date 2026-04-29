using UnityEngine;

public class LluviaManager : MonoBehaviour
{
    private SoundEmitter soundEmitter;

    void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();
    }

    void Start()
    {
        soundEmitter.EmitSound(ESound.LLUVIA);

        // Forzar 2D después de que SoundEmitter lo configure
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
        {
            if (s.clip != null && s.isPlaying)
                s.spatialBlend = 0f; // 2D global
        }
    }
}