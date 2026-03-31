using UnityEngine;

public class LevelManagerEscena2 : MonoBehaviour
{
    [SerializeField] string scene;
    public void CambiarEscena()
    {
        GameManager.Instance.ChangeScene(scene);
    }
}