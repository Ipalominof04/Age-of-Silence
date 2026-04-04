using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }


    // Eliminar: public float soundRadius = 10f;

    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);
        if (soundToEmmit.sound == ESound.NONE)
            Debug.LogError($"No sound found for {sound}");

        float volumeToPlay = volume != -1 ? volume : soundToEmmit.volume;
        SoundManager.Instance.EmitSound(transform.position, volumeToPlay, shouldEmmit);
        return volumeToPlay;
    }

}
