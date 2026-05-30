using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
// L'ESound es un enum que llista tots els sons del joc agrupats per categoria (jugador, Aiden, enemics, ambient i musica)
public enum ESound
{
    // Sorolls PLAYER
    NONE, WALK, RUN, SHOOT, BOTTLE, ROCK, PICKUP,
    // Sorolls AIDEN
    A_SCARE, A_RUN,
    // Sorolls ENEMY
    ENEMY_SHOOT, ENEMY_SHOOT2, ENEMY_PATROL, ENEMY_CHASE,
    // Sorolls AMBIENTE Y GENERALES
    CERILLAS, GASOLINA, FUEGO, PUERTA_PUZZLE, LLUVIA, RIO, BOSQUE, EXPLOSION, ALARMA, RIO2,
    // Sorolls MUSICA
    MUSICAMENU, MUSICACHASE, MUSICACAPITULO, MUSICAAMBIENTE, MUSICAAMBIENTE2, MUSICACREDITOS
}
// El SoundData es una struct serialitzable que agrupa tota la informacio d'un so: el clip, els volums i si es musica
[Serializable]
public struct SoundData
{
    public ESound sound;
    public AudioClip clip;
    public float enemyRangeVolume;
    public float soundVolume;
    public bool isMusic;
}
// El SoundRegistry es un ScriptableObject que actua com a base de dades de tots els sons del joc
[CreateAssetMenu(menuName = "Add Sound registry", fileName = "SoundRegistry")]
public class SoundRegistry : ScriptableObject
{
    public List<SoundData> sounds = new();
    // El GetSound busca i retorna el SoundData corresponent a un ESound donat
    public SoundData GetSound(ESound sound)
    {
        foreach (SoundData soundData in sounds)
        {
            if (soundData.sound == sound) return soundData;
        }
        return new SoundData { };
    }
}