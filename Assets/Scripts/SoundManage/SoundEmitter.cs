// SoundEmitter.cs
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSourceLoop;
    private AudioSource audioSourceOneShot;
    private void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            audioSourceLoop = sources[0];
            audioSourceOneShot = sources[1];
        }
        else if (sources.Length == 1)
        {
            audioSourceLoop = sources[0];
            audioSourceOneShot = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceOneShot = gameObject.AddComponent<AudioSource>();
        }
        audioSourceLoop.outputAudioMixerGroup = SoundManager.Instance.GetMasterGroup();
        audioSourceOneShot.outputAudioMixerGroup = SoundManager.Instance.GetMasterGroup();
        audioSourceLoop.spatialBlend = 1f;
        audioSourceLoop.loop = true;
        audioSourceOneShot.spatialBlend = 1f;
        audioSourceLoop.maxDistance = 8f;
        audioSourceLoop.minDistance = 1f;
        audioSourceLoop.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSourceOneShot.maxDistance = 8f;
        audioSourceOneShot.minDistance = 1f;
        audioSourceOneShot.rolloffMode = AudioRolloffMode.Logarithmic;
    }
    public void SetRange(float minDistance, float maxDistance)
    {
        audioSourceLoop.minDistance = minDistance;
        audioSourceLoop.maxDistance = maxDistance;
        audioSourceOneShot.minDistance = minDistance;
        audioSourceOneShot.maxDistance = maxDistance;

        // Regenerar curva si ya está activo
        if (audioSourceLoop.isPlaying)
        {
            audioSourceLoop.rolloffMode = AudioRolloffMode.Custom;
            audioSourceLoop.SetCustomCurve(
                AudioSourceCurveType.CustomRolloff,
                AnimationCurve.EaseInOut(minDistance, 1f, maxDistance, 0f)
            );
        }
    }
    public void StopSound()
    {
        audioSourceLoop.Stop();
        audioSourceLoop.clip = null;
    }
    private bool IsReplaceable(ESound sound)
    {
        return sound == ESound.WALK || sound == ESound.RUN || sound == ESound.LLUVIA || sound == ESound.FUEGO || sound == ESound.BOSQUE || sound == ESound.RIO || sound == ESound.RIO2 ||
            sound == ESound.A_RUN || sound == ESound.ENEMY_PATROL || sound == ESound.ENEMY_CHASE | sound == ESound.ALARMA;
    }
    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        if (!shouldEmmit) return 0f;
        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);
        if (soundToEmmit.clip == null) return 0f;

        float playerVolumeToPlay = (volume != -1 ? volume : soundToEmmit.volumenOutGame) * SoundManager.Instance.playerVolumeMultiplier;
        float gameVolumeToEmit = soundToEmmit.volumenInGame;

        if (IsReplaceable(sound))
        {
            // Si el clip cambió o no está sonando, inicializar
            if (audioSourceLoop.clip != soundToEmmit.clip || !audioSourceLoop.isPlaying)
            {
                audioSourceLoop.clip = soundToEmmit.clip;
                audioSourceLoop.rolloffMode = AudioRolloffMode.Custom;
                audioSourceLoop.SetCustomCurve(
                    AudioSourceCurveType.CustomRolloff,
                    AnimationCurve.EaseInOut(audioSourceLoop.minDistance, 1f, audioSourceLoop.maxDistance, 0f)
                );
                audioSourceLoop.Play();
            }
            // Actualizar volumen SIEMPRE, esté o no recién iniciado
            audioSourceLoop.volume = playerVolumeToPlay;
        }
        else
        {
            audioSourceOneShot.rolloffMode = AudioRolloffMode.Custom;
            audioSourceOneShot.SetCustomCurve(
                AudioSourceCurveType.CustomRolloff,
                AnimationCurve.EaseInOut(audioSourceOneShot.minDistance, 1f, audioSourceOneShot.maxDistance, 0f)
            );
            audioSourceOneShot.PlayOneShot(soundToEmmit.clip, playerVolumeToPlay);
        }

        SoundManager.Instance.EmitSound(transform.position, gameVolumeToEmit, shouldEmmit);
        return gameVolumeToEmit;
    }
}