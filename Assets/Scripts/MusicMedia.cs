using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Música inicial")]
    [SerializeField] private string startKey;
    private int chaseCount = 0;
    private ESound currentSound = ESound.NONE;

    private bool isChasing = false;

    private void Start()
    {
        if (!string.IsNullOrEmpty(startKey))
            OnMusicEvent(startKey);
    }


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
        if (soundEmitter == null) return;

        if (key == "musicChase")
        {
            print("bool de chase" + isChasing);
            if (!isChasing)
            {
                isChasing = true;
                print("musica de chase");
                soundEmitter.EmitSound(musicDictionary["musicChase"]);
            }
            return;
        }

        if (key == "musicStop")
        {
            isChasing = false;
            soundEmitter.StopSound();
            soundEmitter.EmitSound(currentSound);
            return;
        }

        if (!musicDictionary.TryGetValue(key, out ESound sound))
        {
            Debug.LogWarning($"MusicMedia: clave '{key}' no encontrada.");
            return;
        }

        soundEmitter.StopSound();
        soundEmitter.EmitSound(sound);
        currentSound = sound;
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