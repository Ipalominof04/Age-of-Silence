using UnityEngine;

public class LevelManagerScene2 : MonoBehaviour
{
    [SerializeField] string scene;
    public void CambiarEscena()
    {
        GameManager.Instance.ChangeScene(scene);
    }
}