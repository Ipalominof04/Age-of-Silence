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


    public void StopSound()
    {
        audioSource.Stop();
    }

    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);

        if (soundToEmmit.sound == ESound.NONE)
            return 0f;

        float volumeToPlay = volume != -1 ? volume : soundToEmmit.volume;

        if (soundToEmmit.clip != null)
        {
            audioSource.volume = volumeToPlay;

            if (audioSource.clip != soundToEmmit.clip)
                audioSource.clip = soundToEmmit.clip;

            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(soundToEmmit.clip, volumeToPlay);
        }

        SoundManager.Instance.EmitSound(transform.position, volumeToPlay, shouldEmmit);

        return volumeToPlay;
    }



}
