using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEventFloat")]
public class GameEventFloat : ScriptableObject
{
    public event Action<float> OnEventRaised;

    public void Raise(float value)
    {
        OnEventRaised?.Invoke(value);
    }

    public void Subscribe(Action<float> listener)
    {
        OnEventRaised += listener;
    }

    public void Unsubscribe(Action<float> listener)
    {
        OnEventRaised -= listener;
    }

}
