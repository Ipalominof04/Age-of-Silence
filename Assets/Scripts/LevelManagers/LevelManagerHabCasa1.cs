using UnityEngine;

public class LevelManagerHabCasa1 : MonoBehaviour
{
    [SerializeField] string scene;
    public void CambiarEscena()
    {
        GameManager.Instance.ChangeScene(scene);
    }
}