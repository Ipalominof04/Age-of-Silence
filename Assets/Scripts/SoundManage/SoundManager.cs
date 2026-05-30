using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    public int SubscriberCount => listeners.Count;
    public static SoundManager Instance { get; private set; }
    public SoundRegistry soundRegistry;
    [Range(0f, 1f)]
    public float playerVolumeMultiplier = 1f;
    [Range(0f, 1f)]
    public float musicVolumeMultiplier = 1f;
    [Range(0f, 1f)]
    public float masterVolumeMultiplier = 1f;
    private List<ISoundListener> listeners = new List<ISoundListener>();
    // L'Awake inicialitza el singleton, carrega els volums guardats al PlayerPrefs i els aplica a l'AudioMixer
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (PlayerPrefs.HasKey("playerVolume"))
            playerVolumeMultiplier = PlayerPrefs.GetFloat("playerVolume");
        if (PlayerPrefs.HasKey("musicVolume"))
            musicVolumeMultiplier = PlayerPrefs.GetFloat("musicVolume");
        if (PlayerPrefs.HasKey("masterVolume"))
            masterVolumeMultiplier = PlayerPrefs.GetFloat("masterVolume");
        ApplySoundVolume();
        ApplyMusicVolume();
        ApplyMasterVolume();
    }
    // El Subscribe afegeix un listener a la llista si encara no hi es
    public void Subscribe(ISoundListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }
    // L'Unsubscribe elimina un listener de la llista si hi es
    public void Unsubscribe(ISoundListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
    // L'EmitSound notifica tots els listeners que estiguin dins del radi del so, amb el volum atenuat segons la distancia
    public void EmitSound(Vector3 position, float volume, bool shouldEmmit = true)
    {
        if (!shouldEmmit) return;
        foreach (var listener in listeners)
        {
            if (listener == null) continue;
            float distance = Vector3.Distance(listener.GetPosition(), position);
            float listenRadius = listener.GetListenRadius();
            if (distance < listenRadius)
            {
                float attenuatedVolume = volume * (1f - (distance / listenRadius));
                listener.OnHearSound(position, attenuatedVolume);
            }
        }
    }
    // El SetPlayerVolume guarda el nou volum del jugador al PlayerPrefs i l'aplica a l'AudioMixer
    public void SetPlayerVolume(float value)
    {
        playerVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("playerVolume", playerVolumeMultiplier);
        ApplySoundVolume();
    }
    // L'ApplySoundVolume converteix el volum del jugador a decibels i l'envia a l'AudioMixer
    private void ApplySoundVolume()
    {
        float db = playerVolumeMultiplier > 0
            ? Mathf.Log10(playerVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("SoundVolume", db);
    }
    // El SetMusicVolume guarda el nou volum de la m�sica al PlayerPrefs i l'aplica a l'AudioMixer
    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("musicVolume", musicVolumeMultiplier);
        ApplyMusicVolume();
    }
    // L'ApplyMusicVolume converteix el volum de la m�sica a decibels i l'envia a l'AudioMixer
    private void ApplyMusicVolume()
    {
        float db = musicVolumeMultiplier > 0
            ? Mathf.Log10(musicVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("MusicVolume", db);
    }
    // El SetMasterVolume guarda el nou volum master al PlayerPrefs i l'aplica a l'AudioMixer
    public void SetMasterVolume(float value)
    {
        masterVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("masterVolume", masterVolumeMultiplier);
        ApplyMasterVolume();
    }
    // L'ApplyMasterVolume converteix el volum master a decibels i l'envia a l'AudioMixer
    private void ApplyMasterVolume()
    {
        float db = masterVolumeMultiplier > 0
            ? Mathf.Log10(masterVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("MasterVolume", db);
    }
    // El GetSoundsGroup retorna el grup "Sounds" de l'AudioMixer
    public AudioMixerGroup GetSoundsGroup()
    {
        return audioMixer.FindMatchingGroups("Sounds")[0];
    }
    // El GetMusicGroup retorna el grup "Music" de l'AudioMixer
    public AudioMixerGroup GetMusicGroup()
    {
        return audioMixer.FindMatchingGroups("Music")[0];
    }
    // El GetMasterGroup retorna el grup "Master" de l'AudioMixer
    public AudioMixerGroup GetMasterGroup()
    {
        return audioMixer.FindMatchingGroups("Master")[0];
    }
}