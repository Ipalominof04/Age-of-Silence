using System;
using UnityEngine;

public enum ScareType { Body, Shoot }

[CreateAssetMenu(fileName = "GameEventScareType", menuName = "Events/GameEventScareType")]
public class GameEventScareType : ScriptableObject
{
    public event Action<ScareType> OnEventRaised;

    public void Raise(ScareType type)
    {
        OnEventRaised?.Invoke(type);
    }

    public void Subscribe(Action<ScareType> listener)
    {
        OnEventRaised += listener;
    }

    public void Unsubscribe(Action<ScareType> listener)
    {
        OnEventRaised -= listener;
    }

}
