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
    public Image fadeImage;
    public float fadeDuration = 1f;
    [Header("Spawn")]
    public string lastDoorID = "";

    [Header("Guardado")]
    public string ruta;
    public SOGeneral datos;

    private bool escenaGuardada = false;
    public bool IsChangingScene = false;

    public float GetHealthAlEntrarEscena() => healthAlEntrarEscena;
    public float GetConfianzaAlEntrarEscena() => confianzaAlEntrarEscena;

    // En el awake del game Manager fem la instancia i guardardem la ruta del .json de les dades
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
    // Aquesta funcio es truca cada vegada que es carrega quant es cambia d'escena, s'encarrega de d'esactivar als personatges en les escenes de menu i restaurar dades
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsChangingScene = false;
        if (scene.name == "MainMenu")
        {
            if (PlayerController.Instance != null)
                PlayerController.Instance.gameObject.SetActive(false);
            if (AidenController.Instance != null)
                AidenController.Instance.gameObject.SetActive(false);

            // Quan anem a la escena de 'MainMenu' busquem el canvas de MainMenu i l'activem.
            MainMenuCanvas menu = FindFirstObjectByType<MainMenuCanvas>(FindObjectsInactive.Include);
            if (menu != null)
                menu.mainMenuUI.SetActive(true);
            return; 
        }

        // Depenent de quina sigui l'escena objectiu ens conve no tenir el player actiu, com per exemple en les cinematiques
        if (scene.name == "Creditos" || scene.name == "Escena1" || scene.name == "EscenaFinal")
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.gameObject.SetActive(false);
                AidenController.Instance.gameObject.SetActive(false);
                return;
            }
        }
        // Truquem a la funcio de RestaurarEscena per posar les dades a l'escena
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

        // guardem les dades el SO de dades a les variables corresponents
        healthAlEntrarEscena = datos.PlayerHealth;
        confianzaAlEntrarEscena = datos.confianza;
        datos.PlayerHealth = healthAlEntrarEscena;
        datos.confianza = confianzaAlEntrarEscena;

        // truquem a les funcions de respawn del player i l'aiden
        PlayerController.Instance?.RespawnReset();
        AidenController.Instance?.RespawnReset();

    }

    // Aquesta funcio serveix per augmentar el valor de la confianca amb l'Aiden
    public void SumValor(int amount)
    {
        confianza += amount;
        datos.confianza = confianza;
    }
    // Aquesta funcio serveix per restar el valor de la confianca amb l'Aiden
    public void RestValor(int amount)
    {
        confianza -= amount;
        datos.confianza = confianza;
    }

    // Comprovem si la llista de puzzles conte el puzzle que volem resoldre i si no esta l'insserim
    public void ResolverPuzzle(string puzzleID)
    {
        if (!PuzzleResuelto(puzzleID))
            datos.puzzlesResueltos.Add(puzzleID);
    }
    // Comprovem si la llista conte el puzzle que volem.
    public bool PuzzleResuelto(string puzzleID)
    {
        return datos.puzzlesResueltos.Contains(puzzleID);
    }

    // Amb aquesta funcio controlem en quines escenas volem guardar o no, i aixi, no caure en loops de carrega
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
    // Aquesta funcío s'utilitza per cambiar d'escena pero sense guardar res.
    public void IrAScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }


    // Aquesta es una de les escenes importants per el sistema de guardat i carrega, comprova si hi han objectes per guardar i si hi ha truca a la funcio interna de guardar.
    public void GuardarEscena()
    {
        var objetos = FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in objetos)
        {
            Debug.Log($"Guardando: {obj.name}");
            obj.Guardar();
        }
    }
    // Aqui el que fem es una vegada que l'escena a carregat truquem a Restaurar() per els valors dels objectes
    public void RestaurarEscena()
    {
        foreach (var obj in FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            obj.Restaurar();
    }
    // Amb aquesta funcio borrem les dades dels SOEScena exceptuan els que tenim controlats amb el bool
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
    // Aquesta funcio borra totes les dades dels SOEscena
    public void LimpiarSOEscenasTotal()
    {
        datos.NameScene = "";
        foreach (SOEscena soEscena in datos.escenas)
            soEscena.objetos.Clear();
    }
    // Aquesta funcio es la que activa el fade quan cambiem d'escena, a mes tambe s'encarrega de presentar el nom del capitols quan toca.
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        IsChangingScene = true;
        yield return StartCoroutine(FadeOut());


        var loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
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
    // Aquesta funcio la fem servir per actualitzar la vinyeta de dany utilitzant la referencia de la vida del player
    private void ActualizarVignetteHealth()
    {
        if (DamageVignette.Instance != null && DamageVignette.Instance.IsVignetteActive)
        {
            float ratio = datos.PlayerHealth > 0 ? datos.PlayerHealth / 6f : 1f;
            DamageVignette.Instance.UpdateHealthRatio(ratio);
        }
    }
    // Aquesta funcio es truca cuant l'escena objectiu carrega, el que fa es desactivar l'imatge donant l'efecte de fade out
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
    // Aquesta funcio fa el mateix casi que l'anterior, pero aquesta es truca abans de cambiar.
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
    // Aquesta funcio serveix per moure al player al doorID de l'escena corresponent,
    public void MoverPlayerSpawn()
    {


        if (string.IsNullOrEmpty(lastDoorID)) return;

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);


        foreach (SpawnPoint spawn in spawnPoints)
        {

            if (spawn.GetDoorID() == lastDoorID)
            {

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
                return;
            }
        }
    }
    // Aquesta es altra funcio molt important per el sistema de guardar, en aquesta funcio guardem al json els SOEscena
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
        // Usem el constructor per enmagatzemar les dades
        SaveData data = new SaveData(datos.confianza, datos.PlayerHealth, datos.objetoMano, datos.NameScene, lastDoorID, datos.aidenTieneOso, datos.puzzlesResueltos, escenasSave);

        string jsondata = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(ruta, jsondata);
        }
        catch (Exception ex)
        {
            print("Error al guardar: " + ex.Message);
        }
    }
    // Amb aquest funcio llegim el json i truquem a RestaurarDatos() que es la funcio que s'encarregara de restaurar dades als objectes.
    public void CargarPartida()
    {
        try
        {
            if (!File.Exists(ruta))
            {
                return;
            }
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
    // La funcio de RestaurarDatos() reb un SaveData que es un struct de dades, quan es truca asigna les dades del struct a les variables corresponents.
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
        IrAScene(data.nameScene);
    }

    // Aquest es el struct del SOGeneral, aqui guardaem totes les variables importants que volem que es guardin del nostre joc
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
    // Aquest struct es per crear els SOEscena o te el no de l'escena y els diferents objectes d'aquesta
    [Serializable]
    public struct EscenaData
    {
        public string sceneName;
        public ScenePart[] objetos;
    }
}


