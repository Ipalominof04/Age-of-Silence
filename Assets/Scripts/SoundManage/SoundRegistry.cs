using System;
using System.Collections.Generic;
using UnityEngine;

public enum ESound
{
    NONE, WALK, RUN, CROUCH, SHOOT, BOTTLE, ROCK,
    A_SCARE, A_RUN,
    ENEMY_SHOOT,ENEMY_ALERT,ENEMY_SEARCH
}

[Serializable]
public struct SoundData
{
    public ESound sound;
    public AudioClip clip;
    public float volume;

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
