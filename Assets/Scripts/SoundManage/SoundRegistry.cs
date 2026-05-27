using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum ESound
{
    // SONIDOS PLAYER
    NONE, WALK, RUN, SHOOT, BOTTLE, ROCK,PICKUP,
    // SONIDOS AIDEN
    A_SCARE, A_RUN,
    // SONIDOS ENEMY
    ENEMY_SHOOT,ENEMY_SHOOT2, ENEMY_PATROL, ENEMY_CHASE,
    // SONIDOS AMBIENTE Y GENERALES
    CERILLAS,GASOLINA,FUEGO,PUERTA_PUZZLE,LLUVIA,RIO,BOSQUE,EXPLOSION,ALARMA,RIO2,
    // SONIDOS MUSICA
    MUSICAMENU,MUSICACHASE, MUSICAMURO, MUSICAAMBIENTE, MUSICAAMBIENTE2, MUSICACREDITOS
}

[Serializable]
public struct SoundData
{
    public ESound sound;
    public AudioClip clip;
    public float enemyRangeVolume;
    public float soundVolume;
    public bool isMusic;
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
