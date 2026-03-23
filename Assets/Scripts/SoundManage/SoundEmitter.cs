using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Radio de detecci?n")]
    public float soundRadius = 10f;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }


    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);
        if (soundToEmmit.sound == ESound.NONE)
        {
            Debug.LogError($"No sound found for {sound}");
        }

        float volumeToPlay = volume != -1 ? volume : soundToEmmit.volume;


        //   audioSource.PlayOneShot(soundToEmmit.getRandomClip(), volumeToPlay);

        SoundManager.Instance.EmitSound(transform.position, volumeToPlay, soundRadius, shouldEmmit);

        return volumeToPlay;
    }

}
