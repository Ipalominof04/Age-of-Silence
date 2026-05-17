using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Confianza")]
    [SerializeField] public float confianza = 0;
    private float healthAlEntrarEscena;
    [Header("Fade")]
    public Image fadeImage;        // la UI Image negra
    public float fadeDuration = 1f;
    [Header("Spawn")]
    public string lastDoorID = ""; // guarda la puerta de la que venimos

    [Header("Guardado")]
    public string ruta = "Assets/savegame.json";
    public SOGeneral datos;

    public float GetHealthAlEntrarEscena() => healthAlEntrarEscena;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LimpiarSOEscenas();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == "Creditos")
        {
            Debug.Log($"[GameManager] Escena cargada: '{scene.name}'");
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.gameObject.SetActive(false);
                AidenController.Instance.gameObject.SetActive(false);
                return;
            }

            if (scene.name == "MainMenu")
            {
                MainMenuCanvas menu = FindFirstObjectByType<MainMenuCanvas>(FindObjectsInactive.Include);
                if (menu != null)
                    menu.mainMenuUI.SetActive(true);
                else
                    Debug.LogError("[GameManager] No se encontró MainMenuCanvas");
            }
        }

        RestaurarEscena();

        // 1. Activa primero para que todos los componentes estén listos
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SetActive(true);
            AidenController.Instance.gameObject.SetActive(true);
        }


        // 2. Recoloca después de activar
        MoverPlayerSpawn();

        healthAlEntrarEscena = datos.PlayerHealth;
        Debug.Log($"Vida al entrar escena guardada: {healthAlEntrarEscena}");
        datos.PlayerHealth = healthAlEntrarEscena;

        // 3. Resetea al final cuando todo está listo
        PlayerController.Instance?.RespawnReset();
        AidenController.Instance?.RespawnReset();

        Debug.Log($"[SPAWN] Vida al entrar escena: {healthAlEntrarEscena} | Vida actual: {datos.PlayerHealth}");

        DamageVignette.Instance?.SetVignetteActive(true);
        float ratio = datos.PlayerHealth > 0 ? datos.PlayerHealth / 6f : 1f;
        DamageVignette.Instance?.UpdateHealthRatio(ratio);
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
    public void ResolverPuzzle(string puzzleID)
    {
        if (!datos.puzzlesResueltos.Contains(puzzleID))
            datos.puzzlesResueltos.Add(puzzleID);
    }

    public bool PuzzleResuelto(string puzzleID)
    {
        return datos.puzzlesResueltos.Contains(puzzleID);
    }
    public void ChangeScene(string sceneName)
    {
        if (sceneName != "MainMenu" && sceneName != "Creditos")
        {
            GuardarEscena();
            datos.NameScene = sceneName;
            GuardarPartida();
        }
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    public void IrAScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    public void GuardarEscena()
    {
        var objetos = FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log($"Objetos encontrados: {objetos.Length}");

        foreach (var obj in objetos)
        {
            Debug.Log($"Guardando: {obj.name}");
            obj.Guardar();
        }
    }
    public void RestaurarEscena()
    {
        //    Debug.Log("RestaurarEscena llamado desde:\n" + System.Environment.StackTrace);
        foreach (var obj in FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            obj.Restaurar();
    }
    public void LimpiarSOEscenas()
    {
        datos.NameScene = "";
        foreach (SOEscena soEscena in datos.escenas)
            soEscena.objetos.Clear();
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


        if (sceneName == "Escena2")
        {
            ChapterUI.ChapterStep[] steps = new ChapterUI.ChapterStep[]
            {
                new ChapterUI.ChapterStep("CAPÍTULO 1: LA CIUDAD", 1f),
                new ChapterUI.ChapterStep("LA CIUDAD 1.1: ZONA 0", 1f)
            };
            yield return StartCoroutine(ChapterUI.Instance.PlaySequence(steps));
        }
        else if (sceneName == "Escena4")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("LA CIUDAD 1.2: LA FÁBRICA", 3f));
        }
        else if (sceneName == "SalidaFabrica")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("LA CIUDAD 1.3: ESCAPADA", 3f));
        }
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        Color c = fadeImage.color;

        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.parent.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        Color c = fadeImage.color;

        fadeImage.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
        fadeImage.transform.parent.gameObject.SetActive(false);
    }

    private void MoverPlayerSpawn()
    {
        Debug.Log($"[SPAWN] lastDoorID: '{lastDoorID}'");

        if (string.IsNullOrEmpty(lastDoorID)) return;

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        Debug.Log($"[SPAWN] SpawnPoints encontrados: {spawnPoints.Length}");

        foreach (SpawnPoint spawn in spawnPoints)
        {
            Debug.Log($"[SPAWN] Comparando '{spawn.GetDoorID()}' con '{lastDoorID}'");
            if (spawn.GetDoorID() == lastDoorID)
            {
                Debug.Log($"[SPAWN] ¡Match! Moviendo player a {spawn.transform.position}");
                GameObject player = PlayerController.Instance.gameObject;
                GameObject aiden = AidenController.Instance.gameObject;

                if (player != null)
                {
                    player.transform.position = spawn.transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, spawn.transform.eulerAngles.y, 0f);
                }

                if (aiden != null)
                {
                    NavMeshAgent agent = aiden.GetComponent<NavMeshAgent>();
                    Vector3 targetPos = spawn.transform.position + (-spawn.transform.right * spawn.AidenOffset);

                    if (agent != null)
                    {
                        //desactivar navmesh antes de mover a Aiden para q no lo coloque en el nodo mas cercano a la pos anterior
                        // y poder colocar a Aiden donde quiero
                        agent.enabled = false;
                        aiden.transform.position = targetPos;
                        aiden.transform.rotation = Quaternion.Euler(0f, spawn.transform.eulerAngles.y, 0f);
                        agent.enabled = true;
                    }
                    else
                    {
                        aiden.transform.position = spawn.transform.position + (-spawn.transform.right * spawn.AidenOffset);
                        aiden.transform.rotation = Quaternion.Euler(0f, spawn.transform.eulerAngles.y, 0f);
                    }
                }
                Debug.LogWarning($"[SPAWN] No se encontró SpawnPoint con doorID '{lastDoorID}' en escena '{SceneManager.GetActiveScene().name}'");
                return;
            }
        }
    }

    public void GuardarPartida()
    {
        EscenaData[] escenasSave = new EscenaData[datos.escenas.Length];
        for (int i = 0; i < datos.escenas.Length; i++)
        {
            escenasSave[i] = new EscenaData
            {
                sceneName = datos.escenas[i].sceneName,
                objetos = datos.escenas[i].objetos.ToArray()
            };
        }

        SaveData data = new SaveData(datos.confianza, datos.PlayerHealth, datos.objetoMano, datos.NameScene, datos.puzzlesResueltos, escenasSave);

        string jsondata = JsonUtility.ToJson(data, true);
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
        Debug.Log("CargarPartida llamado");
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

        if (datos.objetoMano != null && !string.IsNullOrEmpty(data.objetoMano))
            datos.objetoMano.nombreObjeto = data.objetoMano;

        datos.puzzlesResueltos = data.puzlesCompletados;

        // Rellena los SOEscena con los datos del JSON ANTES de cargar la escena
        if (data.escenas != null)
        {
            foreach (EscenaData escenaGuardada in data.escenas)
            {
                foreach (SOEscena soEscena in datos.escenas)
                {
                    if (soEscena.sceneName == escenaGuardada.sceneName)
                    {
                        soEscena.objetos.Clear();
                        foreach (var obj in escenaGuardada.objetos)
                            soEscena.objetos.Add(obj);
                        break;
                    }
                }
            }
        }

        datos.NameScene = data.nameScene;
        Debug.Log("Yendo a escena: " + data.nameScene);
        IrAScene(data.nameScene);
    }


    [Serializable]
    public struct SaveData
    {
        public float confianza;
        public float playerHealth;
        public string objetoMano;
        public string nameScene;
        public List<string> puzlesCompletados;
        public EscenaData[] escenas;

        public SaveData(float confianza, float playerHealth, ObjetoAgarrado objetoMano, string nameScene, List<string> puzlesCompletados, EscenaData[] escenas)
        {
            this.confianza = confianza;
            this.playerHealth = playerHealth;
            this.objetoMano = objetoMano != null ? objetoMano.nombreObjeto : "";
            this.nameScene = nameScene;
            this.puzlesCompletados = puzlesCompletados;
            this.escenas = escenas;
        }

    }
    [Serializable]
    public struct EscenaData
    {
        public string sceneName;
        public ScenePart[] objetos;
    }
}


