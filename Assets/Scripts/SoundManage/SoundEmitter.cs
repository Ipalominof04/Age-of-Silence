using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSourceLoop;   // para WALK, RUN, CROUCH
    private AudioSource audioSourceOneShot; // para SHOOT, etc.

    private void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceOneShot = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            audioSourceLoop = sources[0];
            audioSourceOneShot = sources[1];
        }

        audioSourceLoop.spatialBlend = 1f;
        audioSourceOneShot.spatialBlend = 1f;
    }

    public void StopSound()
    {
        audioSourceLoop.Stop();
        audioSourceLoop.clip = null;
    }

    private bool IsReplaceable(ESound sound)
    {
        return sound == ESound.WALK || sound == ESound.RUN || sound == ESound.CROUCH;
    }

    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        if (!shouldEmmit) return 0f;

        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);
        if (soundToEmmit.clip == null) return 0f;

        float volumeToPlay = volume != -1 ? volume : soundToEmmit.volume;

        if (IsReplaceable(sound))
        {
            if (audioSourceLoop.clip != soundToEmmit.clip || !audioSourceLoop.isPlaying)
            {
                audioSourceLoop.clip = soundToEmmit.clip;
                audioSourceLoop.volume = volumeToPlay;
                audioSourceLoop.Play();
            }
        }
        else
        {
            audioSourceOneShot.PlayOneShot(soundToEmmit.clip, volumeToPlay);
        }

        SoundManager.Instance.EmitSound(transform.position, volumeToPlay, shouldEmmit);
        return volumeToPlay;
    }
}