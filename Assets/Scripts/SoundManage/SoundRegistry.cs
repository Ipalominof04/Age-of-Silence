using System;
using System.Collections.Generic;
using UnityEngine;

public enum ESound
{
    NONE, WALK, RUN, CROUCH, SHOOT
}

[Serializable]
public struct RandomClips
{
    public List<AudioClip> clips;
}

[Serializable]
public struct SoundData
{
    public ESound sound;
    public RandomClips clip;
    public float volume;

    //  public AudioClip getRandomClip()
    //  {
    //      return clip.clips[UnityEngine.Random.Range(0, clip.clips.Count)];
    //  }
}

[CreateAssetMenu(menuName = "Add Sound registry", fileName = "SoundRegistry")]
public class SoundRegistry : ScriptableObject
{
    public List<SoundData> sounds = new();

    public SoundData GetSound(ESound sound)
    {
        foreach (SoundData soundData in sounds)
        {
            if (soundData.sound == sound) return soundData;
        }

        return new SoundData { };
    }
}
