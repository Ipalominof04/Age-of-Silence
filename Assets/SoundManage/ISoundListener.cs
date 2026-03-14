using UnityEngine;

public interface ISoundListener
{
    void OnHearSound(Vector3 soundPosition, float volume);
    Vector3 GetPosition();

}
