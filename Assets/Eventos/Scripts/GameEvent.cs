using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    public event Action OnEventRaised;

    public void Raise()
    {
        OnEventRaised?.Invoke();
    }

    public void Subscribe(Action listener)
    {
        OnEventRaised += listener;
    }

    public void Unsubscribe(Action listener)
    {
        OnEventRaised -= listener;
    }

}
