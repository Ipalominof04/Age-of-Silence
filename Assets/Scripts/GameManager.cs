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
    private float confianzaAlEntrarEscena;

    [Header("Fade")]
    public Image fadeImage;        // la UI Image negra
    public float fadeDuration = 1f;
    [Header("Spawn")]
    public string lastDoorID = ""; // guarda la puerta de la que venimos

    [Header("Guardado")]
    public string ruta;
    public SOGeneral datos;

    private bool escenaGuardada = false;
    public bool IsChangingScene = false;

    public float GetHealthAlEntrarEscena() => healthAlEntrarEscena;
    public float GetConfianzaAlEntrarEscena() => confianzaAlEntrarEscena;


    private void Awake()
    {
        ruta = Path.Combine(Application.persistentDataPath, "savegame.json");
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
        IsChangingScene = false;
        if (scene.name == "MainMenu")
        {
            if (PlayerController.Instance != null)
                PlayerController.Instance.gameObject.SetActive(false);
            if (AidenController.Instance != null)
                AidenController.Instance.gameObject.SetActive(false);

            // Buscar con Include para encontrarlo aunque esté desactivado
            MainMenuCanvas menu = FindFirstObjectByType<MainMenuCanvas>(FindObjectsInactive.Include);
            if (menu != null)
                menu.mainMenuUI.SetActive(true);
            else
                Debug.LogError("[GameManager] No se encontró MainMenuCanvas");

            return; // salimos aquí, nada más que hacer en el menú
        }

        if (scene.name == "Creditos" || scene.name == "Escena1" || scene.name == "EscenaFinal")
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

        if (!escenaGuardada)
        {
            if (datos.objetoMano != null)
                datos.objetoMano.nombreObjeto = "";
        }

        escenaGuardada = false;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SetActive(true);
            AidenController.Instance.gameObject.SetActive(true);
        }

        MoverPlayerSpawn();


        healthAlEntrarEscena = datos.PlayerHealth;
        confianzaAlEntrarEscena = datos.confianza;
        datos.PlayerHealth = healthAlEntrarEscena;
        datos.confianza = confianzaAlEntrarEscena;

        PlayerController.Instance?.RespawnReset();
        AidenController.Instance?.RespawnReset();

        Debug.Log($"[SPAWN] Vida al entrar escena: {healthAlEntrarEscena} | Vida actual: {datos.PlayerHealth}");


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
        PlayerController.Instance?.ClearObjetoLanzable();
        if (sceneName != "MainMenu" && sceneName != "Creditos")
        {
            GuardarEscena();
            datos.NameScene = sceneName;
            GuardarPartida();
        }
        else if (sceneName == "MainMenu")
        {
            datos.aidenTieneOso = false;
        }
        escenaGuardada = true;

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
        {
            for (int i = soEscena.objetos.Count - 1; i >= 0; i--)
            {
                if (!soEscena.objetos[i].persisteAlMorir)
                    soEscena.objetos.RemoveAt(i);
            }
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        IsChangingScene = true;
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
                new ChapterUI.ChapterStep("ZONA 0", 1f)
            };
            yield return StartCoroutine(ChapterUI.Instance.PlaySequence(steps));
        }
        else if (sceneName == "Escena4")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("LA FABRICA", 2f));
        }
        else if (sceneName == "SalidaFabrica")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("ESCAPE", 2f));
        }
        else if (sceneName == "SalidaMuro")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("CAPÍTULO 2: EL BOSQUE", 2f));
        }
        else if (sceneName == "EscenaFinal")
        {
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("CAPÍTULO 3: EL DESENLACE", 2f));
        }
        yield return StartCoroutine(FadeIn());
        IsChangingScene = false;
        ActualizarVignetteHealth();
    }

    private void ActualizarVignetteHealth()
    {
        if (DamageVignette.Instance != null && DamageVignette.Instance.IsVignetteActive)
        {
            float ratio = datos.PlayerHealth > 0 ? datos.PlayerHealth / 6f : 1f;
            DamageVignette.Instance.UpdateHealthRatio(ratio);
        }
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

        SaveData data = new SaveData(datos.confianza, datos.PlayerHealth, datos.objetoMano, datos.NameScene, lastDoorID, datos.aidenTieneOso, datos.puzzlesResueltos, escenasSave);

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
            if (!File.Exists(ruta))
            {
                Debug.LogWarning("[GameManager] No existe fichero de guardado en: " + ruta);
                return;
            }
            print("Cargando partida...");
            string jsonData = File.ReadAllText(ruta);
            SaveData data = JsonUtility.FromJson<SaveData>(jsonData);
            escenaGuardada = true;
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
        lastDoorID = data.lastDoorID;
        datos.aidenTieneOso = data.aidenTieneOso;
        AidenController.Instance.SetOso(data.aidenTieneOso);

        if (datos.objetoMano != null)
        {
            datos.objetoMano.ClearObject();
            if (!string.IsNullOrEmpty(data.objetoMano))
                datos.objetoMano.nombreObjeto = data.objetoMano;
        }

        datos.puzzlesResueltos = data.puzlesCompletados;

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
        public string lastDoorID;
        public bool aidenTieneOso;
        public List<string> puzlesCompletados;
        public EscenaData[] escenas;

        public SaveData(float confianza, float playerHealth, ObjetoAgarrado objetoMano, string nameScene, string lastDoorID, bool aidenTieneOso, List<string> puzlesCompletados, EscenaData[] escenas)
        {
            this.confianza = confianza;
            this.playerHealth = playerHealth;
            this.objetoMano = objetoMano != null ? objetoMano.nombreObjeto : "";
            this.nameScene = nameScene;
            this.lastDoorID = lastDoorID;
            this.aidenTieneOso = aidenTieneOso;
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


