using UnityEngine;

public class LluviaManager : MonoBehaviour
{
    private SoundEmitter soundEmitter;

    [Range(0f, 1f)]
    [SerializeField] private float volumenLluvia = 1f;

    void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();
    }

    void Start()
    {
        soundEmitter.EmitSound(ESound.LLUVIA, true, volumenLluvia);

        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
        {
            if (s.clip != null && s.isPlaying)
                s.spatialBlend = 0f;
        }
    }
}