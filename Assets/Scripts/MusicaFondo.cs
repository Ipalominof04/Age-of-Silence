using System.Collections;
using UnityEngine;

public class MusicaFondo : MonoBehaviour
{
    [SerializeField] public SoundEmitter soundEmitter;

    private void OnEnable()
    {
        StartCoroutine(EmitirSiguienteFrame());
    }

    private IEnumerator EmitirSiguienteFrame()
    {
        yield return null; // espera un frame

        soundEmitter.EmitSound(ESound.MUSICAMENU);
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var s in sources)
        {
            if (s.clip != null && s.isPlaying)
                s.spatialBlend = 0f;
        }
        print("suena");
    }
}