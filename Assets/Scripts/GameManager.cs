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

    // En el Awake fem la instancia del singleton i guardem la ruta del fitxer .json on s'emmagatzemaran les dades
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

    // L'OnSceneLoaded es truca automaticament cada vegada que es carrega una escena nova
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsChangingScene = false;

        // si estem al menu principal desactivem el player i l'Aiden i activem la UI del menu
        if (scene.name == "MainMenu")
        {
            if (PlayerController.Instance != null)
                PlayerController.Instance.gameObject.SetActive(false);
            if (AidenController.Instance != null)
                AidenController.Instance.gameObject.SetActive(false);

            MainMenuCanvas menu = FindFirstObjectByType<MainMenuCanvas>(FindObjectsInactive.Include);
            if (menu != null)
                menu.mainMenuUI.SetActive(true);
            return;
        }

        // en escenes de cinematica o credits tampoc volem el player actiu
        if (scene.name == "Creditos" || scene.name == "Escena1" || scene.name == "EscenaFinal")
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.gameObject.SetActive(false);
                AidenController.Instance.gameObject.SetActive(false);
                return;
            }
        }

        // restaurem l'estat de tots els objectes guardables de l'escena
        RestaurarEscena();

        // si la partida no estava guardada, buidem l'objecte a la ma del jugador
        if (!escenaGuardada)
        {
            if (datos.objetoMano != null)
                datos.objetoMano.nombreObjeto = "";
        }

        escenaGuardada = false;

        // activem el player i l'Aiden un cop l'escena esta llesta
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SetActive(true);
            AidenController.Instance.gameObject.SetActive(true);
        }

        // guardem un snapshot de la salut i la confianca al entrar a l'escena per si cal restaurar-los
        healthAlEntrarEscena = datos.PlayerHealth;
        confianzaAlEntrarEscena = datos.confianza;
        datos.PlayerHealth = healthAlEntrarEscena;
        datos.confianza = confianzaAlEntrarEscena;

        // fem el respawn del player i l'Aiden a la posicio correcta
        PlayerController.Instance?.RespawnReset();
        AidenController.Instance?.RespawnReset();
    }

    // El SumValor augmenta la confianca i sincronitza el valor al SO de dades
    public void SumValor(int amount)
    {
        confianza += amount;
        datos.confianza = confianza;
    }

    // El RestValor resta la confianca i sincronitza el valor al SO de dades
    public void RestValor(int amount)
    {
        confianza -= amount;
        datos.confianza = confianza;
    }

    // El ResolverPuzzle afegeix un puzzle a la llista de completats si encara no hi era
    public void ResolverPuzzle(string puzzleID)
    {
        if (!PuzzleResuelto(puzzleID))
            datos.puzzlesResueltos.Add(puzzleID);
    }

    // El PuzzleResuelto comprova si un puzzle ja ha estat completat
    public bool PuzzleResuelto(string puzzleID)
    {
        return datos.puzzlesResueltos.Contains(puzzleID);
    }

    // El ChangeScene guarda l'estat actual, actualitza l'escena objectiu i inicia el canvi amb fade
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

    // L'IrAScene canvia d'escena directament sense guardar res
    public void IrAScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // El GuardarEscena busca tots els objectes guardables de l'escena i els demana que guardin el seu estat
    public void GuardarEscena()
    {
        var objetos = FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in objetos)
        {
            Debug.Log($"Guardando: {obj.name}");
            obj.Guardar();
        }
    }

    // El RestaurarEscena busca tots els objectes guardables i els restaura al seu estat guardat
    public void RestaurarEscena()
    {
        foreach (var obj in FindObjectsByType<ObjetoGuardable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            obj.Restaurar();
    }

    // El LimpiarSOEscenas neteja les dades dels SOEscena pero respecta els objectes marcats com a persistents
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

    // El LimpiarSOEscenasTotal esborra absolutament totes les dades de tots els SOEscena sense excepcions
    public void LimpiarSOEscenasTotal()
    {
        datos.NameScene = "";
        foreach (SOEscena soEscena in datos.escenas)
            soEscena.objetos.Clear();
    }

    // El FadeAndLoadScene gestiona tot el proces de canvi d'escena: fa el fade out, carrega l'escena, mostra el titol del capitol si cal i fa el fade in
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        IsChangingScene = true;
        yield return StartCoroutine(FadeOut());

        // iniciem la carrega de l'escena sense activar-la encara
        var loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null) { yield break; }

        // esperem que la carrega arribi al 90% abans d'activar l'escena
        loadOperation.allowSceneActivation = false;
        while (loadOperation.progress < 0.9f) yield return null;
        loadOperation.allowSceneActivation = true;
        Debug.Log($"{sceneName} is loaded at 90%");

        while (!loadOperation.isDone) yield return null;
        Debug.Log($"{sceneName} is loaded at 100%");

        // establim la nova escena com a activa
        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid() && newScene.isLoaded) SceneManager.SetActiveScene(newScene);

        // mostrem el titol del capitol corresponent a cada escena
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
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("LA FABRICA", 2f));
        else if (sceneName == "SalidaFabrica")
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("ESCAPE", 2f));
        else if (sceneName == "SalidaMuro")
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("CAPÍTULO 2: EL BOSQUE", 2f));
        else if (sceneName == "EscenaFinal")
            yield return StartCoroutine(ChapterUI.Instance.MostrarCapitulo("CAPÍTULO 3: EL DESENLACE", 2f));

        yield return StartCoroutine(FadeIn());
        IsChangingScene = false;
        ActualizarVignetteHealth();
    }

    // L'ActualizarVignetteHealth actualitza la vinyeta de dany de la pantalla amb la salut actual del jugador
    private void ActualizarVignetteHealth()
    {
        if (DamageVignette.Instance != null && DamageVignette.Instance.IsVignetteActive)
        {
            float ratio = datos.PlayerHealth > 0 ? datos.PlayerHealth / 6f : 1f;
            DamageVignette.Instance.UpdateHealthRatio(ratio);
        }
    }

    // El FadeOut va augmentant progressivament l'opacitat de la imatge negra fins a tapar tota la pantalla
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

    // El FadeIn fa el contrari que el FadeOut, va reduint l'opacitat fins a deixar la pantalla visible i desactiva la imatge
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

    // El MoverPlayerSpawn busca el SpawnPoint que coincideix amb el lastDoorID i col·loca el player i l'Aiden a la seva posicio
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
                        // desactivem el NavMesh abans de moure l'Aiden per evitar que el NavMesh el relocatzi al node mes proper
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

    // El GuardarPartida converteix totes les dades importants del joc a JSON i les escriu al fitxer de guardada
    public void GuardarPartida()
    {
        // construim un array amb les dades de totes les escenes per serialitzar-les
        EscenaData[] escenasSave = new EscenaData[datos.escenas.Length];
        for (int i = 0; i < datos.escenas.Length; i++)
        {
            escenasSave[i] = new EscenaData
            {
                sceneName = datos.escenas[i].sceneName,
                objetos = datos.escenas[i].objetos.ToArray()
            };
        }
        // creem el struct de SaveData amb totes les dades i el convertim a JSON
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

    // El CargarPartida llegeix el fitxer JSON, el deserialitza i crida RestaurarDatos per aplicar les dades al joc
    public void CargarPartida()
    {
        try
        {
            if (!File.Exists(ruta)) { return; }
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

    // El RestaurarDatos rep un SaveData i distribueix cada valor guardat a la variable correcta del joc
    private void RestaurarDatos(SaveData data)
    {
        // restaurem la confianca tant a la variable local com al SO de dades
        datos.confianza = data.confianza;
        confianza = datos.confianza;

        // restaurem la salut del jugador i el lastDoorID per saber on fer el spawn
        datos.PlayerHealth = data.playerHealth;
        lastDoorID = data.lastDoorID;

        // restaurem si l'Aiden porta l'oso i l'apliquem al controlador
        datos.aidenTieneOso = data.aidenTieneOso;
        AidenController.Instance.SetOso(data.aidenTieneOso);

        // restaurem l'objecte que tenia el jugador a la ma
        if (datos.objetoMano != null)
        {
            datos.objetoMano.ClearObject();
            if (!string.IsNullOrEmpty(data.objetoMano))
                datos.objetoMano.nombreObjeto = data.objetoMano;
        }

        // restaurem la llista de puzzles completats
        datos.puzzlesResueltos = data.puzlesCompletados;

        // restaurem l'estat de cada objecte de cada escena buscant la coincidencia pel nom de l'escena
        if (data.escenas != null)
        {
            foreach (EscenaData escenaGuardada in data.escenas)
            {
                foreach (SOEscena soEscena in datos.escenas)
                {
                    if (soEscena.sceneName == escenaGuardada.sceneName)
                    {
                        // buidem la llista actual i la reomplim amb les dades guardades
                        soEscena.objetos.Clear();
                        foreach (var obj in escenaGuardada.objetos)
                            soEscena.objetos.Add(obj);
                        break;
                    }
                }
            }
        }

        // finalment anem a l'escena on estava el jugador quan va guardar
        datos.NameScene = data.nameScene;
        IrAScene(data.nameScene);
    }

    // El SaveData es el struct principal de guardada, conté totes les variables importants que necessitem persistir entre sessions
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

    // L'EscenaData es un struct auxiliar que emmagatzema el nom d'una escena i tots els seus objectes guardables
    [Serializable]
    public struct EscenaData
    {
        public string sceneName;
        public ScenePart[] objetos;
    }
}