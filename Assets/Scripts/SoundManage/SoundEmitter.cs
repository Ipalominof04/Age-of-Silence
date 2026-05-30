using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSourceLoop;
    private AudioSource audioSourceOneShot;
    // L'Awake inicialitza els dos AudioSource (loop i oneshot) i els configura amb l'espai 3D, distàncies i el grup de l'AudioMixer
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
    // El SetRange canvia les distàncies mínima i màxima d'ambdós AudioSource i regenera la corba si el loop ja està sonant
    public void SetRange(float minDistance, float maxDistance)
    {
        audioSourceLoop.minDistance = minDistance;
        audioSourceLoop.maxDistance = maxDistance;
        audioSourceOneShot.minDistance = minDistance;
        audioSourceOneShot.maxDistance = maxDistance;
        if (audioSourceLoop.isPlaying)
        {
            audioSourceLoop.rolloffMode = AudioRolloffMode.Custom;
            audioSourceLoop.SetCustomCurve(
                AudioSourceCurveType.CustomRolloff,
                AnimationCurve.EaseInOut(minDistance, 1f, maxDistance, 0f)
            );
        }
    }
    // El StopSound atura el loop i neteja el clip per deixar l'AudioSource buit
    public void StopSound()
    {
        audioSourceLoop.Stop();
        audioSourceLoop.clip = null;
    }
    // L'IsReplaceable indica si un so ha de reproduir-se en loop (sons continus com caminar, música, pluja, etc.)
    private bool IsReplaceable(ESound sound)
    {
        return sound == ESound.WALK || sound == ESound.RUN || sound == ESound.LLUVIA || sound == ESound.FUEGO || sound == ESound.BOSQUE || sound == ESound.RIO || sound == ESound.RIO2 ||
            sound == ESound.A_RUN || sound == ESound.ENEMY_PATROL || sound == ESound.ENEMY_CHASE | sound == ESound.ALARMA ||
            sound == ESound.MUSICAMENU || sound == ESound.MUSICACHASE || sound == ESound.MUSICAAMBIENTE || sound == ESound.MUSICAAMBIENTE2 || sound == ESound.MUSICACAPITULO || sound == ESound.MUSICACREDITOS;
    }
    // L'EmitSound reprodueix un so en loop o en oneshot segons el tipus, aplica el volum i notifica el SoundManager de la posició
    public float EmitSound(ESound sound, bool shouldEmmit = true, float volume = -1)
    {
        if (!shouldEmmit) return 0f;
        var soundToEmmit = SoundManager.Instance.soundRegistry.GetSound(sound);
        if (soundToEmmit.clip == null) return 0f;
        float playerVolumeToPlay = (volume != -1 ? volume : soundToEmmit.soundVolume) * SoundManager.Instance.playerVolumeMultiplier;
        float gameVolumeToEmit = soundToEmmit.enemyRangeVolume;
        if (IsReplaceable(sound))
        {
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
            audioSourceLoop.spatialBlend = soundToEmmit.isMusic ? 0f : 1f;
            audioSourceLoop.outputAudioMixerGroup = soundToEmmit.isMusic
            ? SoundManager.Instance.GetMusicGroup()
            : SoundManager.Instance.GetSoundsGroup();
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