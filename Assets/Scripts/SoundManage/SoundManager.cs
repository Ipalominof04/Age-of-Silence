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
    public void Subscribe(ISoundListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }
    public void Unsubscribe(ISoundListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
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
    public void SetPlayerVolume(float value)
    {
        playerVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("playerVolume", playerVolumeMultiplier);
        ApplySoundVolume();
    }
    private void ApplySoundVolume()
    {
        float db = playerVolumeMultiplier > 0
            ? Mathf.Log10(playerVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("SoundVolume", db);
    }
    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("musicVolume", musicVolumeMultiplier);
        ApplyMusicVolume();
    }
    private void ApplyMusicVolume()
    {
        float db = musicVolumeMultiplier > 0
            ? Mathf.Log10(musicVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("MusicVolume", db);
    }
    public void SetMasterVolume(float value)
    {
        masterVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("masterVolume", masterVolumeMultiplier);
        ApplyMasterVolume();
    }
    private void ApplyMasterVolume()
    {
        float db = masterVolumeMultiplier > 0
            ? Mathf.Log10(masterVolumeMultiplier) * 20f
            : -80f;
        audioMixer.SetFloat("MasterVolume", db);
    }
    public AudioMixerGroup GetSoundsGroup()
    {
        return audioMixer.FindMatchingGroups("Sounds")[0];
    }
    public AudioMixerGroup GetMusicGroup()
    {
        return audioMixer.FindMatchingGroups("Music")[0];
    }
    public AudioMixerGroup GetMasterGroup()
    {
        return audioMixer.FindMatchingGroups("Master")[0];
    }
}