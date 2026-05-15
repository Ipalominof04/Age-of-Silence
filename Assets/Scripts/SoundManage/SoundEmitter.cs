using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicMedia : MonoBehaviour
{
    [Serializable]
    public struct MusicEntry
    {
        public string key;
        public ESound sound;
    }

    [Header("Evento")]
    [SerializeField] private GameEventString musicEvent;

    [Header("Diccionario de músicas")]
    [SerializeField] private List<MusicEntry> musicEntries = new();

    [Header("Activar reproducción")]
    [SerializeField] private bool canPlay = true;

    private Dictionary<string, ESound> musicDictionary;

    private SoundEmitter soundEmitter;

    private void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();

        // Crear diccionario
        musicDictionary = new Dictionary<string, ESound>();

        foreach (var entry in musicEntries)
        {
            if (!musicDictionary.ContainsKey(entry.key))
            {
                musicDictionary.Add(entry.key, entry.sound);
            }
        }
    }

    private void OnEnable()
    {
        if (musicEvent != null)
        {
            musicEvent.Subscribe(OnMusicEvent);
        }
    }

    private void OnDisable()
    {
        if (musicEvent != null)
        {
            musicEvent.Unsubscribe(OnMusicEvent);
        }
    }

    private void OnMusicEvent(string key)
    {
        if (!canPlay) return;

        if (!musicDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"No existe una música con la clave: {key}");
            return;
        }

        ESound sound = musicDictionary[key];

        switch (sound)
        {
            case ESound.MUSICAMENU:
            case ESound.MUSICACHASE:
                soundEmitter.EmitSound(sound);
                break;

            default:
                Debug.LogWarning($"El sonido {sound} no es música válida.");
                break;
        }
    }

    public void EnableMusic(bool value)
    {
        canPlay = value;

        if (!value)
        {
            soundEmitter.StopSound();
        }
    }
}