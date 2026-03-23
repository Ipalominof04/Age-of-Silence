using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public int SubscriberCount => listeners.Count;

    public static SoundManager Instance { get; private set; }

    public SoundRegistry soundRegistry;

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

    public void EmitSound(Vector3 position, float volume, float radius, bool shouldEmmit = true)
    {
        if (!shouldEmmit) return;
        foreach (var listener in listeners)
        {
            if (listener == null) continue;
            float distance = Vector3.Distance(listener.GetPosition(), position);
            if (distance < radius)
            {
                float attenuatedVolume = volume * (1f - (distance / radius));
                listener.OnHearSound(position, attenuatedVolume);
            }
        }
    }

}
