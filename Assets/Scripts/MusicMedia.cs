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
    private ESound currentSound = ESound.NONE;
    private bool isChasing = false;
    // L'Start reprodueix la música inicial si s'ha definit una clau de inici
    private void Start()
    {
        if (!string.IsNullOrEmpty(startKey))
            OnMusicEvent(startKey);
    }
    // L'Awake obté el SoundEmitter i construeix el diccionari de músiques a partir de la llista serialitzada
    private void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();
        musicDictionary = new Dictionary<string, ESound>();
        foreach (var entry in musicEntries)
        {
            if (!musicDictionary.ContainsKey(entry.key))
            {
                musicDictionary.Add(entry.key, entry.sound);
            }
        }
    }
    // L'OnEnable se subscriu a l'event de música quan l'objecte s'activa
    private void OnEnable()
    {
        if (musicEvent != null)
        {
            musicEvent.Subscribe(OnMusicEvent);
        }
    }
    // L'OnDisable es dessubscriu de l'event de música quan l'objecte es desactiva
    private void OnDisable()
    {
        if (musicEvent != null)
        {
            musicEvent.Unsubscribe(OnMusicEvent);
        }
    }
    // L'OnMusicEvent gestiona els canvis de música segons la clau rebuda, controlant la persecució i la música normal
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
    // L'EnableMusic activa o desactiva la reproducció de música, aturant-la si es desactiva
    public void EnableMusic(bool value)
    {
        canPlay = value;
        if (!value)
        {
            soundEmitter.StopSound();
        }
    }
}