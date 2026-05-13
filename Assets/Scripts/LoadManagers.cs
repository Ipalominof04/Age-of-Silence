using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManagers : MonoBehaviour
{
    public void Start()
    {
        SceneManager.LoadScene("MainMenu");
    }
}