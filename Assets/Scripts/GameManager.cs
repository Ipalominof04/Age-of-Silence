using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Confianza")]
    [SerializeField] public float confianza = 0;
    [Header("Fade")]
    private Image fadeImage;        // la UI Image negra
    public float fadeDuration = 1f;
    [Header("Spawn")]
    public string lastDoorID = ""; // guarda la puerta de la que venimos

    [Header("Guardado")]
    public string ruta = "Assets/savegame.json";
    public SOGeneral datos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Busca la imagen de fade en la escena nueva
        fadeImage = GameObject.Find("Fundido")?.GetComponent<Image>();

        if (fadeImage == null)
        {
            Debug.LogWarning("No se encontro Fundido en la escena");
        }
        else
        {
            // Asegura que empieza transparente
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
        MoverPlayerSpawn();
    }

    public void SumValor(int amount)
    {
        confianza += amount;
        datos.confianza = confianza;
        Debug.Log($"Valor subió a: {confianza}");
    }

    public void RestValor(int amount)
    {
        confianza -= amount;
        datos.confianza = confianza;
        Debug.Log($"Valor bajó a: {confianza}");
    }

    public void ChangeScene(string sceneName)
    {
        datos.NameScene = sceneName;
        StartCoroutine(FadeAndLoadScene(sceneName));
        GuardarPartida();
    }
    
    public void IrAScene(string sceneName)
    {
            StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());

        var loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            Debug.Log($"Error loading {sceneName}");
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        while (loadOperation.progress < 0.9f) yield return null;
        loadOperation.allowSceneActivation = true;
        Debug.Log($"{sceneName} is loaded at 90%");

        while (!loadOperation.isDone) yield return null;
        Debug.Log($"{sceneName} is loaded at 100%");

        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid() && newScene.isLoaded) SceneManager.SetActiveScene(newScene);

        yield return null;
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        Color c = fadeImage.color;
        fadeImage.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (t / fadeDuration));
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }

    private void MoverPlayerSpawn()
    {
        if (string.IsNullOrEmpty(lastDoorID)) return;

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint spawn in spawnPoints)
        {
            if (spawn.GetDoorID() == lastDoorID)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                GameObject aiden = GameObject.FindGameObjectWithTag("Aiden");

                if (player != null)
                {
                    player.transform.position = spawn.transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, spawn.transform.eulerAngles.y, 0f);
                }

                if (aiden != null)
                {
                    aiden.transform.position = spawn.transform.position + (-spawn.transform.right * spawn.AidenOffset);
                    aiden.transform.rotation = Quaternion.Euler(0f, spawn.transform.eulerAngles.y, 0f);
                }
                return;
            }
        }
    }

    public void GuardarPartida()
    {
        SaveData data = new SaveData(datos.confianza, datos.PlayerHealth, datos.objetoMano, datos.NameScene);

        string jsondata = JsonUtility.ToJson(data);
        try
        {
            print("Guardando datos...");
            File.WriteAllText(ruta, jsondata);
        }
        catch (Exception ex)
        {
            print("Error al guardar: " + ex.Message);
        }
    }
    public void CargarPartida()
    {
        try
        {
            print("Cargando partida...");
            string jsonData = File.ReadAllText(ruta);
            SaveData data = JsonUtility.FromJson<SaveData>(jsonData);
            RestaurarDatos(data);
        }
        catch (Exception ex)
        {
            print("Error al cargar la partida: " + ex.Message);
        }
    }

    private void RestaurarDatos(SaveData data)
    {
        datos.confianza = data.confianza;
        confianza = datos.confianza;
        datos.PlayerHealth = data.playerHealth;

        // Solo asigna si el campo del SO y el string guardado son válidos
        if (datos.objetoMano != null && !string.IsNullOrEmpty(data.objetoMano))
        {
            datos.objetoMano.nombreObjeto = data.objetoMano;
        }

        datos.NameScene = data.nameScene;

        IrAScene("LoadManagers");
    }


    [Serializable]
    public struct SaveData
    {
        public float confianza;
        public float playerHealth;
        public string objetoMano;
        public string nameScene;

        public SaveData(float confianza, float playerHealth, ObjetoAgarrado objetoMano, string nameScene)
        {
            this.confianza = confianza;
            this.playerHealth = playerHealth;
            this.objetoMano = objetoMano != null ? objetoMano.nombreObjeto : "";
            this.nameScene = nameScene;
        }

    }
}


