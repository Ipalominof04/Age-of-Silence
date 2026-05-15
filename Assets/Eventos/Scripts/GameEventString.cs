using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventScareString", menuName = "Events/GameEventString")]
public class GameEventString : ScriptableObject
{
    public event Action<string> OnEventRaised;

    public void Raise(string type)
    {
        OnEventRaised?.Invoke(type);
    }

    public void Subscribe(Action<string> listener)
    {
        OnEventRaised += listener;
    }

    public void Unsubscribe(Action<string> listener)
    {
        OnEventRaised -= listener;
    }
}
