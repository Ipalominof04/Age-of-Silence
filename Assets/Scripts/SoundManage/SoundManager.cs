using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    public int SubscriberCount => listeners.Count;

    public static SoundManager Instance { get; private set; }

    public SoundRegistry soundRegistry;

    [Range(0f, 5f)]
    public float playerVolumeMultiplier = 1f;

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
        playerVolumeMultiplier = PlayerPrefs.GetFloat("playerVolume", 1f);

        float db = playerVolumeMultiplier > 0
           ? Mathf.Log10(playerVolumeMultiplier) * 20f
           : -80f;
        audioMixer.SetFloat("MasterVolume", db);

    }

    // Suscribirse
    public void Subscribe(ISoundListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    // Desuscribirse
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
                // Atenuació per distància
                float attenuatedVolume = volume * (1f - (distance / listenRadius));
                // El filtre de nivell mínim el fa el listener
                listener.OnHearSound(position, attenuatedVolume);
            }
        }
    }
    public void SetPlayerVolume(float value)
    {
        playerVolumeMultiplier = Mathf.Clamp(value, 0f, 5f);
        PlayerPrefs.SetFloat("playerVolume", playerVolumeMultiplier);

        // El AudioMixer trabaja en decibelios, hay que convertir
        float db = playerVolumeMultiplier > 0
            ? Mathf.Log10(playerVolumeMultiplier) * 20f
            : -80f; // -80db = silencio total
        audioMixer.SetFloat("MasterVolume", db);
    }
    public AudioMixerGroup GetMasterGroup()
    {
        return audioMixer.FindMatchingGroups("Master")[0];
    }
}
