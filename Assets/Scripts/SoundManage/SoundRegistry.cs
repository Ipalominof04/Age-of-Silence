using System;
using System.Collections.Generic;
using UnityEngine;

public enum ESound
{
    // SONIDOS PLAYER
    NONE, WALK, RUN, CROUCH, SHOOT, BOTTLE, ROCK,
    // SONIDOS AIDEN
    A_SCARE, A_RUN,
    // SONIDOS ENEMY
    ENEMY_SHOOT,ENEMY_SHOOT2, ENEMY_PATROL, ENEMY_CHASE,
    // SONIDOS EN GENERAL
    CERILLAS,GASOLINA,FUEGO,PUERTA_PUZZLE,LLUVIA
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
