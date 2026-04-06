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


    private void Start()
    {
        SceneManager.LoadScene("Escena3");
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
}
